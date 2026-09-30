const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const vm = require('node:vm');
const path = require('node:path');

function loadOverlay() {
    let receive;
    const warnings = [];
    const context = vm.createContext({
        jQuery: {},
        setInterval() { return 0; },
        queueMicrotask,
        window: { location: { search: '' }, chrome: { webview: {
            addEventListener(type, handler) { receive = handler; }, postMessage() {}
        } } },
        document: { readyState: 'loading', addEventListener() {} },
        console: { log() {}, warn(...args) { warnings.push(args); }, error() {} }
    });
    vm.runInContext(fs.readFileSync(path.join(__dirname, '../src/v2/script.js'), 'utf8'), context);
    return { context, warnings, send(type, payload) { receive({ data: { type, payload } }); } };
}

test('invalid saved regex does not prevent startup', () => {
    const overlay = loadOverlay();
    let connections = 0;
    overlay.context.Chat.connect = channel => { assert.equal(channel, 'test-channel'); connections++; };
    overlay.send('config', { channel: 'test-channel', regex: '[' });
    overlay.send('credentials', { token: 'synthetic-test-token' });
    assert.equal(connections, 1);
    assert.equal(overlay.context.Chat.info.regex, null);
    assert.equal(overlay.warnings.length, 1);
    overlay.send('config', { channel: 'test-channel', regex: '[' });
    assert.equal(connections, 1, 'Successful startup must not duplicate connections');
});

test('valid patterns work and removing a pattern clears it', () => {
    const { context } = loadOverlay();
    context.Chat.applySettings({ regex: '^spam' });
    assert.equal(context.Chat.info.regex.test('spam message'), true);
    assert.equal(context.Chat.info.regex.test('hello'), false);
    context.Chat.applySettings({ regex: '' });
    assert.equal(context.Chat.info.regex, null);
});

test('a startup failure can retry after the host sends corrected settings', () => {
    const overlay = loadOverlay();
    let connections = 0;
    overlay.context.Chat.connect = () => { if (++connections === 1) throw new Error('synthetic startup failure'); };
    overlay.send('credentials', {});
    overlay.send('config', { channel: 'test-channel', regex: '' });
    assert.equal(vm.runInContext('hasStartedNativeChat', overlay.context), false);
    overlay.send('config', { channel: 'test-channel', regex: '^spam' });
    assert.equal(connections, 2);
    assert.equal(vm.runInContext('hasStartedNativeChat', overlay.context), true);
});
