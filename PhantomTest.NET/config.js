(function(window) {
    // ADD YOUR TEST MODULES HERE (or you can use single module and use requirejs to load up more as needed)
    // For example var testModules = ['./test/test1', './test/test2', './test/test3']
    var testModules = ['cs!./test/example1', 'cs!./test/example2'];

    // RequireJS config... baseUrl should be relative to your test.html file to match up to your
    // require js base of the classes you use for testing, should also include your paths/shims
    // I have left an example config here for you to overwrite
    var config = {
        baseUrl: './app',
        paths: {
            'knockout': 'http://ajax.aspnetcdn.com/ajax/knockout/knockout-2.1.0',
            'jquery': 'http://ajax.googleapis.com/ajax/libs/jquery/1.8.0/jquery.min'
        },
        shim: {
        }
    };

    // Do test runner type configuration here
    mocha.setup({ globals: ['$', 'jQuery', 'XMLHttpRequest'] });
    mocha.ui('bdd');
    mocha.reporter('html');
    expect = chai.expect;
    sinon.config = {
        useFakeTimers: true,
        useFakeServer: true
    };

    //////////////////////////////////////////////
    // DONT NEED TO TOUCH ANYTHING BELOW
    //////////////////////////////////////////////

    // Run mocha tests or use phantomjs
    define(testModules, function(utils) {
        if (window.mochaPhantomJS) {
            mochaPhantomJS.run();
        } else {
            mocha.run();
        }
    });

    // helper function to replace requirejs modules
    createContext = function (stubs, name, cb) {
        //create a new map which will override the path to a given dependencies
        //so if we have a module in m1, requiresjs will look now unter   
        //stub_m1
        var map = {};

        // Undefine old stubs and make sure we reset the map key
        if (requirejs.currentStubs != null) {
            _.each(requirejs.currentStubs, function (key) {
                key = key.substring(key.indexOf('!') + 1);
                var stubname = 'stub_' + key;
                requirejs.undef(stubname);
                map[key] = key;
            });
        }

        // If we have stubs overwrite the previous keys if applicable
        if(stubs != null) {
            _.each(stubs, function (value, key) {
                key = key.substring(key.indexOf('!') + 1);
                var stubname = 'stub_' + key;
                map[key] = stubname;
            });
        }

        // make sure the current module maps to itself!
        map[name] = name;

        // create a new context with the new dependency paths
        var newConfig = _.extend({}, config);
        newConfig.map = {
            '*': map
        };

        // set the requirejs config - this will have our stub mapping in it
        requirejs.config(newConfig);

        // create new definitions that will return our passed stubs or mocks
        if(stubs != null) {
            requirejs.currentStubs = [];
            _.each(stubs, function (value, key) {
                key = key.substring(key.indexOf('!') + 1);
                var stubname = 'stub_' + key;
                requirejs.currentStubs.push(key);

                define(stubname, [], function() {
                    return value;
                });
            });
        }

        // Undefine the current module and reload it
        requirejs.undef(name);
        return require([name], cb);
    };
})(window);