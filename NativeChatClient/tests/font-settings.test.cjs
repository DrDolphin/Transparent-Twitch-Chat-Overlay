const test = require('node:test');
const assert = require('node:assert/strict');
const loadForm = require('./helpers/settings-form.cjs');

test('every preset font index survives opening and saving Appearance', () => {
    const form = loadForm();
    for (let index = 0; index < form.fonts.indexOf('Custom'); index++) {
        assert.equal(form.roundTrip(String(index)), String(index));
        assert.equal(form.roundTrip(index), String(index));
    }
});

test('named preset fonts remain compatible and custom names round-trip', () => {
    const form = loadForm();
    assert.equal(form.roundTrip(form.fonts[2]), '2');
    for (const font of ['Fira Code', 'My Custom Font, monospace']) assert.equal(form.roundTrip(font), font);
});

test('missing and out-of-range preset values fall back to the default', () => {
    const form = loadForm();
    for (const font of [undefined, null, '', '999']) assert.equal(form.roundTrip(font), '0');
});
