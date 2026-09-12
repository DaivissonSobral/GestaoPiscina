// Inscrição do navegador em notificações Web Push (VAPID). Não usa DotNetObjectReference
// porque é um fluxo de requisição/resposta única (pedir permissão + inscrever), sem eventos
// contínuos para reportar de volta ao Blazor.
window.gestaoPiscinaPush = {
    isSupported: function () {
        return 'serviceWorker' in navigator && 'PushManager' in window;
    },

    // Retorna a inscrição (formato de PushSubscription.toJSON()) já existente ou uma nova,
    // ou null se o suporte não existir ou a permissão for negada.
    subscribe: async function (vapidPublicKey) {
        if (!this.isSupported()) {
            return null;
        }

        const registration = await navigator.serviceWorker.ready;
        let subscription = await registration.pushManager.getSubscription();

        if (!subscription) {
            if (Notification.permission === 'denied') {
                return null;
            }

            const permission = await Notification.requestPermission();
            if (permission !== 'granted') {
                return null;
            }

            subscription = await registration.pushManager.subscribe({
                userVisibleOnly: true,
                applicationServerKey: urlBase64ToUint8Array(vapidPublicKey)
            });
        }

        return subscription.toJSON();
    }
};

// PushManager.subscribe exige a chave VAPID como Uint8Array, mas o servidor entrega a
// chave em base64url — conversão padrão recomendada pela própria documentação da API.
function urlBase64ToUint8Array(base64String) {
    const padding = '='.repeat((4 - (base64String.length % 4)) % 4);
    const base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/');
    const rawData = window.atob(base64);
    const outputArray = new Uint8Array(rawData.length);

    for (let i = 0; i < rawData.length; ++i) {
        outputArray[i] = rawData.charCodeAt(i);
    }

    return outputArray;
}
