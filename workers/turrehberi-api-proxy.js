const ORIGIN = "https://REPLACE_TOURS_API_ORIGIN";
const ORIGIN_HOST = "REPLACE_TOURS_API_HOST";

export default {
  async fetch(request) {
    const incomingUrl = new URL(request.url);
    const targetUrl = new URL(request.url);
    targetUrl.protocol = "https:";
    targetUrl.hostname = ORIGIN_HOST;
    targetUrl.port = "";

    const headers = new Headers(request.headers);
    headers.set("Host", ORIGIN_HOST);
    headers.set("X-Forwarded-Host", incomingUrl.host);
    headers.set("X-Forwarded-Proto", incomingUrl.protocol.replace(":", ""));

    const proxiedRequest = new Request(targetUrl.toString(), {
      method: request.method,
      headers,
      body: request.body,
      redirect: "manual"
    });

    const response = await fetch(proxiedRequest);
    const proxiedResponse = new Response(response.body, response);
    proxiedResponse.headers.set("Access-Control-Allow-Origin", "https://tours.corplynk.com");
    proxiedResponse.headers.set("Access-Control-Allow-Credentials", "true");
    proxiedResponse.headers.set("X-Proxy-Origin", ORIGIN);
    return proxiedResponse;
  }
};
