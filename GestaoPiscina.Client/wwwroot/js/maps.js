// Integração com Google Maps (Maps JavaScript API + Geocoding), usada em dois lugares:
// 1) Geocodificar o endereço de Clientes/Usuários no momento de salvar (ClienteCadastroModal,
//    UsuarioModal), gravando Latitude/Longitude no banco pra não precisar geocodificar de novo.
// 2) Exibir o mapa de Gestão de Rota (Pages/Rotas.razor), com marcadores de clientes (iniciais)
//    e técnicos/supervisores (foto).
//
// Usa google.maps.Marker (API clássica) em vez de AdvancedMarkerElement de propósito: o
// marcador avançado exige configurar um "Map ID" separado no Google Cloud Console, e queremos
// que a tela funcione só com uma chave de API normal (Maps JavaScript API + Geocoding API).
window.gestaoPiscinaMaps = (function () {
    let scriptPromise = null;
    const maps = {};

    function loadScript(apiKey) {
        // Carregamento clássico (sem "loading=async"/importLibrary): essa combinação se
        // mostrou instável — o onload do script às vezes disparava antes de
        // "google.maps.importLibrary" existir, ou antes de "google.maps.Map" existir,
        // dependendo do estado de cache do navegador. Como só usamos a API clássica
        // (Map, Geocoder, Marker) e não AdvancedMarkerElement, não precisamos do sistema
        // de bibliotecas novo — o script clássico já deixa tudo pronto no onload.
        if (window.google && window.google.maps && window.google.maps.Map && window.google.maps.Geocoder) {
            return Promise.resolve();
        }
        if (scriptPromise) {
            return scriptPromise;
        }
        scriptPromise = new Promise((resolve, reject) => {
            const script = document.createElement('script');
            script.src = 'https://maps.googleapis.com/maps/api/js?key=' + encodeURIComponent(apiKey);
            script.async = true;
            script.onload = () => resolve();
            script.onerror = () => {
                scriptPromise = null;
                reject(new Error('Não foi possível carregar o Google Maps.'));
            };
            document.head.appendChild(script);
        });
        return scriptPromise;
    }

    async function geocode(apiKey, address) {
        if (!address) {
            return null;
        }
        await loadScript(apiKey);
        return new Promise((resolve) => {
            const geocoder = new google.maps.Geocoder();
            geocoder.geocode({ address: address }, (results, status) => {
                if (status === 'OK' && results && results.length > 0) {
                    const loc = results[0].geometry.location;
                    resolve({ lat: loc.lat(), lng: loc.lng() });
                } else {
                    resolve(null);
                }
            });
        });
    }

    async function initMap(elementId, apiKey, centerLat, centerLng, zoom) {
        await loadScript(apiKey);
        const el = document.getElementById(elementId);
        if (!el) {
            return;
        }
        const map = new google.maps.Map(el, {
            center: { lat: centerLat, lng: centerLng },
            zoom: zoom,
            mapTypeControl: false,
            streetViewControl: false,
            fullscreenControl: false
        });
        maps[elementId] = { map: map, markers: [], bounds: new google.maps.LatLngBounds() };
    }

    function clearMarkers(elementId) {
        const entry = maps[elementId];
        if (!entry) {
            return;
        }
        entry.markers.forEach((m) => m.setMap(null));
        entry.markers = [];
        entry.bounds = new google.maps.LatLngBounds();
    }

    function fitToMarkers(elementId) {
        const entry = maps[elementId];
        if (!entry || entry.markers.length === 0) {
            return;
        }
        entry.map.fitBounds(entry.bounds);
        if (entry.markers.length === 1) {
            entry.map.setZoom(15);
        }
    }

    function drawInitialsIcon(initials, color) {
        const size = 40;
        const canvas = document.createElement('canvas');
        canvas.width = size;
        canvas.height = size;
        const ctx = canvas.getContext('2d');
        ctx.beginPath();
        ctx.arc(size / 2, size / 2, size / 2 - 2, 0, 2 * Math.PI);
        ctx.fillStyle = color || '#0891b2';
        ctx.fill();
        ctx.strokeStyle = '#ffffff';
        ctx.lineWidth = 2;
        ctx.stroke();
        ctx.fillStyle = '#ffffff';
        ctx.font = 'bold 14px sans-serif';
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillText((initials || '').toUpperCase(), size / 2, size / 2 + 1);
        return canvas.toDataURL();
    }

    function addMarkerWithIcon(elementId, lat, lng, iconUrl, title, size, onClick) {
        const entry = maps[elementId];
        if (!entry) {
            return;
        }
        const position = { lat: lat, lng: lng };
        const marker = new google.maps.Marker({
            position: position,
            map: entry.map,
            title: title,
            icon: {
                url: iconUrl,
                scaledSize: new google.maps.Size(size, size),
                anchor: new google.maps.Point(size / 2, size / 2)
            }
        });
        if (onClick) {
            marker.addListener('click', onClick);
        }
        entry.markers.push(marker);
        entry.bounds.extend(position);
    }

    function addInitialsMarker(elementId, lat, lng, initials, title, color) {
        addMarkerWithIcon(elementId, lat, lng, drawInitialsIcon(initials, color), title, 40);
    }

    // Igual addInitialsMarker, mas clicável: usado pelos clientes do dia na tela de Gestão
    // de Rota (Pages/Rotas.razor) pra permitir selecionar/desselecionar clientes na hora de
    // montar a rota manualmente de um técnico. dotNetRef é a referência .NET do componente
    // (DotNetObjectReference), chamada de volta via JSInvokable a cada clique.
    function addSelectableInitialsMarker(elementId, lat, lng, initials, title, color, dotNetRef, idCliente) {
        addMarkerWithIcon(elementId, lat, lng, drawInitialsIcon(initials, color), title, 40, () => {
            dotNetRef.invokeMethodAsync('OnClienteMarcadorClicado', idCliente);
        });
    }

    // Recorta a foto em círculo via canvas; se a imagem for de outra origem sem CORS
    // liberado, o canvas fica "contaminado" (toDataURL lança SecurityError) — nesse caso,
    // cai de volta pra usar a URL da foto direto (sem recorte), em vez de falhar o marcador.
    function drawCircularCrop(img, size) {
        const canvas = document.createElement('canvas');
        canvas.width = size;
        canvas.height = size;
        const ctx = canvas.getContext('2d');
        ctx.save();
        ctx.beginPath();
        ctx.arc(size / 2, size / 2, size / 2 - 2, 0, 2 * Math.PI);
        ctx.closePath();
        ctx.clip();
        ctx.drawImage(img, 0, 0, size, size);
        ctx.restore();
        ctx.beginPath();
        ctx.arc(size / 2, size / 2, size / 2 - 2, 0, 2 * Math.PI);
        ctx.strokeStyle = '#0f172a';
        ctx.lineWidth = 2;
        ctx.stroke();
        return canvas.toDataURL();
    }

    // Carrega a foto em duas etapas: primeiro com crossOrigin "anonymous", pra poder
    // recortar em círculo via canvas; se o servidor da foto não enviar cabeçalhos CORS
    // (ex.: acesso via um túnel ainda não liberado em Program.cs), o navegador REJEITA
    // esse carregamento (onerror, nunca onload) em vez de só "contaminar" o canvas — nesse
    // caso caímos pra uma segunda tentativa sem crossOrigin, que sempre carrega (é assim
    // que a foto já aparece em outras telas do sistema), só que sem o recorte circular.
    // Só cai no "?" de iniciais se a foto realmente não existir/carregar de jeito nenhum.
    function addPhotoMarker(elementId, lat, lng, photoUrl, title) {
        if (!photoUrl) {
            addInitialsMarker(elementId, lat, lng, '?', title, '#7c3aed');
            return;
        }

        const size = 44;
        const img = new Image();
        img.crossOrigin = 'anonymous';
        img.onload = () => {
            try {
                addMarkerWithIcon(elementId, lat, lng, drawCircularCrop(img, size), title, size);
            } catch (e) {
                addMarkerWithIcon(elementId, lat, lng, photoUrl, title, size);
            }
        };
        img.onerror = () => {
            const fallbackImg = new Image();
            fallbackImg.onload = () => addMarkerWithIcon(elementId, lat, lng, photoUrl, title, size);
            fallbackImg.onerror = () => addInitialsMarker(elementId, lat, lng, '?', title, '#7c3aed');
            fallbackImg.src = photoUrl;
        };
        img.src = photoUrl;
    }

    // Traça a rota (viagem redonda: sai do técnico, visita os clientes selecionados,
    // volta pro técnico) usando DirectionsService, com optimizeWaypoints pra deixar o
    // Google decidir a melhor ordem de visita entre os pontos escolhidos manualmente
    // (ou sugeridos por proximidade) em Pages/Rotas.razor. preserveViewport porque quem
    // controla o enquadramento do mapa é sempre fitToMarkers, chamado logo depois.
    function calculateRoute(elementId, apiKey, originLat, originLng, waypoints) {
        return loadScript(apiKey).then(() => new Promise((resolve) => {
            const entry = maps[elementId];
            if (!entry) {
                resolve({ sucesso: false, erro: 'Mapa não iniciado' });
                return;
            }
            if (!entry.directionsRenderer) {
                entry.directionsRenderer = new google.maps.DirectionsRenderer({ map: entry.map, suppressMarkers: true, preserveViewport: true });
            }
            const origin = { lat: originLat, lng: originLng };
            const directionsService = new google.maps.DirectionsService();
            directionsService.route({
                origin: origin,
                destination: origin,
                waypoints: waypoints.map((w) => ({ location: { lat: w.lat, lng: w.lng }, stopover: true })),
                optimizeWaypoints: true,
                travelMode: google.maps.TravelMode.DRIVING
            }, (result, status) => {
                if (status === 'OK' && result) {
                    entry.directionsRenderer.setDirections(result);
                    let distanciaMetros = 0;
                    let duracaoSegundos = 0;
                    result.routes[0].legs.forEach((leg) => {
                        distanciaMetros += leg.distance ? leg.distance.value : 0;
                        duracaoSegundos += leg.duration ? leg.duration.value : 0;
                    });
                    resolve({
                        sucesso: true,
                        distanciaMetros: distanciaMetros,
                        duracaoSegundos: duracaoSegundos,
                        ordem: result.routes[0].waypoint_order
                    });
                } else {
                    resolve({ sucesso: false, erro: status });
                }
            });
        }));
    }

    function limparRota(elementId) {
        const entry = maps[elementId];
        if (entry && entry.directionsRenderer) {
            entry.directionsRenderer.setMap(null);
            entry.directionsRenderer = null;
        }
    }

    // Rota simples, de um ponto A até um ponto B (sem waypoints, sem ida-e-volta) — usada
    // pelo RotaOSWidget (Shared/RotaOSWidget.razor) pra mostrar o trajeto do técnico até o
    // cliente na tela de cadastro/detalhes de uma OS específica.
    function tracarRotaSimples(elementId, apiKey, origemLat, origemLng, destinoLat, destinoLng) {
        return loadScript(apiKey).then(() => new Promise((resolve) => {
            const entry = maps[elementId];
            if (!entry) {
                resolve({ sucesso: false, erro: 'Mapa não iniciado' });
                return;
            }
            if (!entry.directionsRenderer) {
                entry.directionsRenderer = new google.maps.DirectionsRenderer({ map: entry.map, suppressMarkers: true, preserveViewport: true });
            }
            const directionsService = new google.maps.DirectionsService();
            directionsService.route({
                origin: { lat: origemLat, lng: origemLng },
                destination: { lat: destinoLat, lng: destinoLng },
                travelMode: google.maps.TravelMode.DRIVING
            }, (result, status) => {
                if (status === 'OK' && result) {
                    entry.directionsRenderer.setDirections(result);
                    let distanciaMetros = 0;
                    let duracaoSegundos = 0;
                    result.routes[0].legs.forEach((leg) => {
                        distanciaMetros += leg.distance ? leg.distance.value : 0;
                        duracaoSegundos += leg.duration ? leg.duration.value : 0;
                    });
                    resolve({ sucesso: true, distanciaMetros: distanciaMetros, duracaoSegundos: duracaoSegundos });
                } else {
                    resolve({ sucesso: false, erro: status });
                }
            });
        }));
    }

    // Geolocalização do navegador/celular (ver OrdemServicoDetalhes.IniciarAsync) — resolve
    // null em vez de rejeitar sempre que não conseguir (sem suporte, permissão negada,
    // timeout etc.), porque isso nunca deve impedir o usuário de iniciar a OS.
    function obterLocalizacaoAtual() {
        return new Promise((resolve) => {
            if (!navigator.geolocation) {
                resolve(null);
                return;
            }
            navigator.geolocation.getCurrentPosition(
                (pos) => resolve({ lat: pos.coords.latitude, lng: pos.coords.longitude }),
                () => resolve(null),
                { enableHighAccuracy: true, timeout: 10000 }
            );
        });
    }

    return {
        geocode: geocode,
        initMap: initMap,
        clearMarkers: clearMarkers,
        fitToMarkers: fitToMarkers,
        addInitialsMarker: addInitialsMarker,
        addSelectableInitialsMarker: addSelectableInitialsMarker,
        addPhotoMarker: addPhotoMarker,
        calculateRoute: calculateRoute,
        tracarRotaSimples: tracarRotaSimples,
        obterLocalizacaoAtual: obterLocalizacaoAtual,
        limparRota: limparRota
    };
})();
