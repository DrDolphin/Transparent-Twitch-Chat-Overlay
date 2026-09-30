const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const vm = require('node:vm');
const path = require('node:path');

function loadOverlay() {
    let receive;
    const context = vm.createContext({
        jQuery: {}, setInterval() { return 0; },
        queueMicrotask,
        IntersectionObserver: class { observe() {} unobserve() {} },
        window: { addEventListener() {}, location: { search: '' }, chrome: { webview: {
            addEventListener(type, handler) { receive = handler; }, postMessage() {}
        } } },
        document: { readyState: 'loading', addEventListener() {} },
        console: { log() {}, warn() {}, error() {} }
    });
    for (const file of ['utils.js', 'script.js'])
        vm.runInContext(fs.readFileSync(path.join(__dirname, '../src/v2', file), 'utf8'), context);
    const writes = [];
    context.Chat.write = (...args) => writes.push(args);
    context.Chat.connect = () => {};
    return { context, writes, send(type, payload) { receive({ data: { type, payload } }); } };
}

test('redemptions wait for configuration then reach the NativeChat renderer', async () => {
    const overlay = loadOverlay();
    const message = 'Redeemed "reward"\n<input> \\ quote\'';
    overlay.send('chatMessage', { message, nick: 'Viewer', color: '#a1b3c4' });
    assert.equal(overlay.writes.length, 0);
    overlay.send('config', { channel: 'test-channel' });
    overlay.send('credentials', {});
    await Promise.resolve();
    assert.equal(overlay.writes.length, 1);
    const [nick, tags, received, service] = overlay.writes[0];
    assert.equal(nick, 'Viewer');
    assert.equal(received, message);
    assert.equal(tags.color, '#a1b3c4');
    assert.match(tags.id, /^host-/);
    assert.equal(service, 'twitch');
    overlay.send('chatMessage', { message: 'second', nick: 'Other' });
    assert.equal(overlay.writes.length, 2);
});

test('display names are escaped and system messages get a usable name', () => {
    const { context, writes } = loadOverlay();
    context.Chat.writeHostMessage({ message: 'safe', nick: '<script>"&' });
    assert.equal(writes[0][1]['display-name'], '&lt;script&gt;"&amp;');
    context.Chat.writeHostMessage({ message: 'status', nick: '' });
    assert.equal(writes[1][0], 'System');
    context.Chat.writeHostMessage(null);
    context.Chat.writeHostMessage({ message: 123 });
    assert.equal(writes.length, 2);
});
