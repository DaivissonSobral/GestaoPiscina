// Em desenvolvimento (dotnet run / dotnet watch) sempre busca da rede e nunca
// armazena nada em cache — evita exatamente o problema que motivou o antigo
// clear-sw.js (alterações no código não aparecendo por causa de cache
// desatualizado). O cache de verdade só existe em service-worker.published.js,
// usado automaticamente no build de publish (ver .csproj).
self.addEventListener('fetch', () => { });

self.addEventListener('push', function (event) {
    const data = event.data ? event.data.json() : {};
    event.waitUntil(self.registration.showNotification(data.title || 'BLUP Inteligência Aquática', {
        body: data.body || '',
        icon: '/icon-192.png',
        data: { url: data.url || '/' }
    }));
});

self.addEventListener('notificationclick', function (event) {
    event.notification.close();
    event.waitUntil(clients.openWindow(event.notification.data && event.notification.data.url || '/'));
});
