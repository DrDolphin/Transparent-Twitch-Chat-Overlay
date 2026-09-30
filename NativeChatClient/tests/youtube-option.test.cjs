const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const loadForm = require('./helpers/settings-form.cjs');

test('legacy YouTube values cannot be re-enabled when saving the form', () => {
    const form = loadForm();
    assert.equal(form.roundTripSettings({ channel: 'twitch-channel', yt: '@old-handle', font: '0' }).yt, '');
    vm.runInContext("$ytChannel.val('@new-handle')", form.context);
    assert.equal(JSON.parse(vm.runInContext('getSettingsData()', form.context)).yt, '');
});

test('the desktop form identifies YouTube chat as unavailable', () => {
    const html = fs.readFileSync(path.join(__dirname, '../src/index.html'), 'utf8');
    const input = html.match(/<input\b[^>]*name="yt-channel"[^>]*>/)[0];
    assert.match(input, /\bdisabled\b/);
    assert.match(input, /YouTube chat is unavailable/);
});

test('the overlay does not load the unsupported YouTube relay client', () => {
    const html = fs.readFileSync(path.join(__dirname, '../src/v2/index.html'), 'utf8');
    assert.doesNotMatch(html, /<script[^>]*src=["'][^"']*youtube\.js/);
});
