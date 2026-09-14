// Recorte circular de foto de perfil (usado no cadastro/edição de Usuário) — carrega a
// imagem escolhida num canvas quadrado, permite arrastar (posicionar o rosto) e ampliar
// (slider de zoom), e exporta o resultado como JPEG base64 já enquadrado.
window.gestaoPiscinaCropper = (function () {
    const state = {};

    // Recebe o conteúdo do arquivo escolhido via DotNetStreamReference (streaming .NET -> JS)
    // e monta um blob local pra usar como fonte da imagem no canvas.
    async function iniciarComStream(canvasId, streamRef) {
        try {
            const arrayBuffer = await streamRef.arrayBuffer();
            const blob = new Blob([arrayBuffer]);
            const url = URL.createObjectURL(blob);
            return await iniciar(canvasId, url);
        } catch (e) {
            console.error("Erro ao carregar imagem a partir do stream", e);
            return false;
        }
    }

    function iniciar(canvasId, objectUrl) {
        return new Promise((resolve) => {
            const canvas = document.getElementById(canvasId);
            if (!canvas) {
                resolve(false);
                return;
            }

            const img = new Image();
            img.onload = () => {
                let entry = state[canvasId];
                if (!entry) {
                    entry = state[canvasId] = { canvas, ctx: canvas.getContext("2d") };
                }

                if (entry.objectUrl) {
                    URL.revokeObjectURL(entry.objectUrl);
                }

                const size = canvas.width;
                const scaleMin = Math.max(size / img.width, size / img.height);
                Object.assign(entry, {
                    img,
                    objectUrl,
                    size,
                    scaleMin,
                    scale: scaleMin,
                    offsetX: 0,
                    offsetY: 0,
                    dragging: false,
                    lastX: 0,
                    lastY: 0
                });

                desenhar(canvasId);
                registrarEventos(canvasId);
                resolve(true);
            };
            img.onerror = () => resolve(false);
            img.src = objectUrl;
        });
    }

    function desenhar(canvasId) {
        const s = state[canvasId];
        if (!s || !s.img) {
            return;
        }

        const { ctx, size, img, scale, offsetX, offsetY } = s;
        ctx.clearRect(0, 0, size, size);
        ctx.save();
        const w = img.width * scale;
        const h = img.height * scale;
        const x = (size - w) / 2 + offsetX;
        const y = (size - h) / 2 + offsetY;
        ctx.drawImage(img, x, y, w, h);

        // Escurece tudo fora do círculo, pra pré-visualizar exatamente como vai ficar
        // depois de aplicado (a mesma máscara circular usada no avatar via CSS).
        ctx.globalCompositeOperation = "destination-in";
        ctx.beginPath();
        ctx.arc(size / 2, size / 2, size / 2, 0, Math.PI * 2);
        ctx.fill();
        ctx.restore();
    }

    function clampOffset(canvasId) {
        const s = state[canvasId];
        if (!s || !s.img) {
            return;
        }

        const w = s.img.width * s.scale;
        const h = s.img.height * s.scale;
        const maxX = Math.max(0, (w - s.size) / 2);
        const maxY = Math.max(0, (h - s.size) / 2);
        s.offsetX = Math.min(maxX, Math.max(-maxX, s.offsetX));
        s.offsetY = Math.min(maxY, Math.max(-maxY, s.offsetY));
    }

    function zoom(canvasId, valorSlider) {
        const s = state[canvasId];
        if (!s || !s.img) {
            return;
        }

        const t = Math.min(100, Math.max(0, valorSlider)) / 100;
        s.scale = s.scaleMin + t * s.scaleMin * 2; // até 3x o zoom mínimo (imagem inteira visível)
        clampOffset(canvasId);
        desenhar(canvasId);
    }

    function registrarEventos(canvasId) {
        const entry = state[canvasId];
        const canvas = entry.canvas;
        if (canvas.__cropperEventsBound) {
            return;
        }
        canvas.__cropperEventsBound = true;

        const pos = (e) => {
            if (e.touches && e.touches.length) {
                return { x: e.touches[0].clientX, y: e.touches[0].clientY };
            }
            return { x: e.clientX, y: e.clientY };
        };

        const down = (e) => {
            const s = state[canvasId];
            if (!s) return;
            const p = pos(e);
            s.dragging = true;
            s.lastX = p.x;
            s.lastY = p.y;
            e.preventDefault();
        };

        const move = (e) => {
            const s = state[canvasId];
            if (!s || !s.dragging) return;
            const p = pos(e);
            s.offsetX += p.x - s.lastX;
            s.offsetY += p.y - s.lastY;
            s.lastX = p.x;
            s.lastY = p.y;
            clampOffset(canvasId);
            desenhar(canvasId);
            e.preventDefault();
        };

        const up = () => {
            const s = state[canvasId];
            if (s) s.dragging = false;
        };

        canvas.addEventListener("mousedown", down);
        window.addEventListener("mousemove", move);
        window.addEventListener("mouseup", up);
        canvas.addEventListener("touchstart", down, { passive: false });
        canvas.addEventListener("touchmove", move, { passive: false });
        canvas.addEventListener("touchend", up);
    }

    function exportar(canvasId, tamanhoSaida) {
        const s = state[canvasId];
        if (!s || !s.img) {
            return null;
        }

        const out = document.createElement("canvas");
        out.width = tamanhoSaida;
        out.height = tamanhoSaida;
        const octx = out.getContext("2d");
        const ratio = tamanhoSaida / s.size;
        const w = s.img.width * s.scale * ratio;
        const h = s.img.height * s.scale * ratio;
        const x = (tamanhoSaida - w) / 2 + s.offsetX * ratio;
        const y = (tamanhoSaida - h) / 2 + s.offsetY * ratio;
        octx.drawImage(s.img, x, y, w, h);

        return out.toDataURL("image/jpeg", 0.9).split(",")[1];
    }

    function descartar(canvasId) {
        const s = state[canvasId];
        if (s && s.objectUrl) {
            URL.revokeObjectURL(s.objectUrl);
            s.objectUrl = null;
        }
    }

    return {
        iniciarComStream: iniciarComStream,
        iniciar: iniciar,
        zoom: zoom,
        exportar: exportar,
        descartar: descartar
    };
})();
