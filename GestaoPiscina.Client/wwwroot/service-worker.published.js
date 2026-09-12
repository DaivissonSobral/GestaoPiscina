// Estratégia de cache baseada no template oficial do Blazor WebAssembly PWA
// (https://learn.microsoft.com/aspnet/core/blazor/progressive-web-app). Usado
// só no build de publish — em desenvolvimento, service-worker.js (sem cache
// nenhum) é quem roda.

// Carrega self.assetsManifest (gerado pelo SDK a partir de ServiceWorkerAssetsManifest
// no .csproj) — sem isso, self.assetsManifest fica undefined e o SW quebra ao tentar
// ler self.assetsManifest.version logo abaixo.
self.importScripts('./service-worker-assets.js');

const cacheNamePrefix = 'offline-cache-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;
const offlineAssetsInclude = [/\.dll$/, /\.pdb$/, /\.wasm/, /\.html$/, /\.js$/, /\.json$/, /\.css$/, /\.woff$/, /\.png$/, /\.jpe?g$/, /\.gif$/, /\.ico$/, /\.blat$/, /\.dat$/];
const offlineAssetsExclude = [/^service-worker\.js$/];

// Se o app for hospedado numa subpasta, troque pelo caminho base correspondente (com '/' no final).
const base = self.registration.scope;
const baseUrl = new URL(base);
const manifestUrlList = self.assetsManifest.assets.map(asset => new URL(asset.url, baseUrl).href);

async function onInstall(event) {
    console.info('Service worker: Install');

    // Baixa e armazena em cache todos os itens do manifesto de assets que batem com os padrões acima.
    const assetsRequests = self.assetsManifest.assets
        .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
        .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
        .map(asset => new Request(asset.url, { integrity: asset.hash, cache: 'no-cache' }));
    await caches.open(cacheName).then(cache => cache.addAll(assetsRequests));
}

async function onActivate(event) {
    console.info('Service worker: Activate');

    // Remove caches de versões antigas.
    const cacheKeys = await caches.keys();
    await Promise.all(cacheKeys
        .filter(key => key.startsWith(cacheNamePrefix) && key !== cacheName)
        .map(key => caches.delete(key)));
}

async function onFetch(event) {
    let cachedResponse = null;
    if (event.request.method === 'GET') {
        // Para requisições de navegação, tenta servir o index.html do cache
        // (necessário para rotas do Blazor Router funcionarem offline),
        // exceto quando a própria requisição já é para um recurso do manifesto.
        const shouldServeIndexHtml = event.request.mode === 'navigate'
            && !manifestUrlList.some(url => url === event.request.url);

        const request = shouldServeIndexHtml ? 'index.html' : event.request;
        const cache = await caches.open(cacheName);
        cachedResponse = await cache.match(request);
    }

    return cachedResponse || fetch(event.request);
}

self.addEventListener('install', event => event.waitUntil(onInstall(event)));
self.addEventListener('activate', event => event.waitUntil(onActivate(event)));
self.addEventListener('fetch', event => event.respondWith(onFetch(event)));

self.addEventListener('push', function (event) {
    const data = event.data ? event.data.json() : {};
    event.waitUntil(self.registration.showNotification(data.title || 'Gestão de Piscinas', {
        body: data.body || '',
        icon: '/icon-192.png',
        data: { url: data.url || '/' }
    }));
});

self.addEventListener('notificationclick', function (event) {
    event.notification.close();
    event.waitUntil(clients.openWindow(event.notification.data && event.notification.data.url || '/'));
});
