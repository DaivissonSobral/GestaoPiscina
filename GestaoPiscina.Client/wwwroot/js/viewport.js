// Espelha o breakpoint "sm" do Tailwind (640px) usado para ocultar os botões de
// alternância Lista/Kanban no celular, permitindo que componentes Blazor reajam
// à mudança sem duplicar o valor do breakpoint em C#.
window.gestaoPiscinaViewport = {
    mobileQuery: window.matchMedia('(max-width: 639px)'),

    isMobile: function () {
        return this.mobileQuery.matches;
    },

    registerMobileChange: function (dotNetRef) {
        this.mobileQuery.addEventListener('change', function (e) {
            dotNetRef.invokeMethodAsync('OnMobileViewportChanged', e.matches);
        });
    }
};
