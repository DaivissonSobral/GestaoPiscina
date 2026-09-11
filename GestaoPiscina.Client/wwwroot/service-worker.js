// Em desenvolvimento (dotnet run / dotnet watch) sempre busca da rede e nunca
// armazena nada em cache — evita exatamente o problema que motivou o antigo
// clear-sw.js (alterações no código não aparecendo por causa de cache
// desatualizado). O cache de verdade só existe em service-worker.published.js,
// usado automaticamente no build de publish (ver .csproj).
self.addEventListener('fetch', () => { });
