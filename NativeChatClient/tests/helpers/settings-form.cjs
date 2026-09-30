const fs = require('node:fs');
const vm = require('node:vm');
const path = require('node:path');

module.exports = function loadForm() {
    const fields = new Map();
    function $(selector) {
        if (!fields.has(selector)) {
            let value = '';
            const properties = {};
            const field = {
                val(next) { if (arguments.length === 0 || next === undefined) return value; value = String(next); return this; },
                prop(key, next) { if (arguments.length === 1) return properties[key]; properties[key] = next; return this; },
                is() { return Boolean(properties.checked); },
                change() { return this; }, submit() { return this; }, click() { return this; }
            };
            fields.set(selector, field);
        }
        return fields.get(selector);
    }
    const context = vm.createContext({ $, console,
        document: { getElementById() { return { style: {} }; }, addEventListener() {} },
        window: { addEventListener() {} }
    });
    const settings = fs.readFileSync(path.join(__dirname, '../../src/settings.js'), 'utf8');
    vm.runInContext(settings.slice(0, settings.indexOf('const sizes')), context);
    vm.runInContext(fs.readFileSync(path.join(__dirname, '../../src/script.js'), 'utf8'), context);
    vm.runInContext('initializePreview = function () {};', context);
    return {
        context,
        fonts: vm.runInContext('fonts', context),
        roundTripSettings(settings) {
            context.savedSettings = settings;
            vm.runInContext('populateFormFromSettings(savedSettings)', context);
            return JSON.parse(vm.runInContext('getSettingsData()', context));
        },
        roundTrip(font) { return this.roundTripSettings({ font }).font; }
    };
};
