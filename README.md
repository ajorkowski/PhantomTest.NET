
# PhantomTest.NET

Uses PhantomJS to run your mocha/chai/sinon test suite inside VS test runners. It uses requirejs to load up your tests,
additionally it provides you the ability to inject your own dependencies if you use requirejs in your application.

## How to install?

Core: Install-Package PhantomTest.NET

NUnit: Install-Package PhantomTest.NET.NUnit

## Usage - NUnit

If you install the PhantomTest.NET.NUnit package you just have to add a test to your test suite:

        [Test]
        [JavascriptTests("../path/to/testdir/")]
        public void RunJavascriptTests(JavascriptTest test)
        {
            test.Assert();
        }

All tests will come up seperately in your NUnit test runner (This includes the inbuilt VS Test runner and NCrunch etc).
Skipped tests (in Mocha) should come up as ignored and failed tests will include the error. I could not work out a
way to include the duration information (however they exist on the JavascriptTest object). 

## Usage - Command Line

You can run the tests anytime from the command line using the RunTests.bat file installed with the core component.

## Usage - Browser

If you need to debug your tests the best way might be running the tests directly in your browser. All you have to do
is open the tests.html file in your favourite browser.

## Why?

I wanted to setup my javascript unit tests to run in my NCrunch test runner but it was a massive pain. My first attempt
was with a pure nodejs solution however this runs into issues when you are using jquery/knockout etc, far too hard 
to mock out. This is why we use phantomjs to run in a browser like environment. I also use requirejs and I really wanted
to be able to be able to 'inject' dependencies for unit testing purposes.

## What gets installed?

The testing suite that is setup by default has the following libraries included:

+ [Mocha](http://visionmedia.github.com/mocha/) - We use the browser version
+ [Chai](http://chaijs.com/) - The assertion library, we use the 'expect' style by default
+ [Sinon](http://sinonjs.org/) - For all your spying/stubing/mocking needs, also great for mocking out ajax calls
+ [Coffeescript](http://coffeescript.org/) - Supported out of the box, you can stub dependencies written in coffeescript too
+ [Underscore](http://underscorejs.org/) - Underscore is available on the global context if needed
+ [RequireJS](http://requirejs.org/) - RequireJS is used to load the modules and also can help you inject dependencies

All test packages has a dependency on the core PhantomTest.NET package which adds a bunch of stuff to your project:

    your project
        app
            example application files
        test
            example test files
        mocha-phantomjs
            core_extensions.js
        dependencies
            dependencies as described above
        coffee-script.js
        config.js
        cs.js
        mocha-phantomjs.coffee
        phantomjs.exe
        runTests.bat
        tests.html

You can run your project using runTests.bat, or simply by running tests.html in your favourite browser. If you use
one of the other test runner packages you can also see your tests come up in your favourite test runner. (Currently
only have an NUnit wrapper...)

You can easily make your own wrappers by taking advantage of the test runner output (Found in the PhantomJS.NET package):

    TestRunner.RunAllTests("/Tests/Base/Path");

## Setup

After installing the module your first (and only) step is to set up the config.js file. You must specify the location of your
modules and the configuration of your *application* level requirejs. Make sure to set the baseUrl to the location
of your application files and include any other paths etc. You can also run your coffee files directly using the cs directive

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

After this there is a section for configuring mocha/chai/sinon

    // Do test runner type configuration here
    mocha.setup({ globals: ['$', 'jQuery', 'XMLHttpRequest'] });
    mocha.ui('bdd');
    mocha.reporter('html');
    expect = chai.expect;
    sinon.config = {
        useFakeTimers: true,
        useFakeServer: true
    };

Now your tests are ready to roll. Obviously as you add tests you will need to add them to your module list.

## Injecting dependencies and loading app modules

In your actual tests you can inject dependencies using the createContext method that is added to the global context.
Here is a standard example you might use:

    define [], ->
        describe 'dep2', ->
            _viewModel = null
            _otherDep = null

            beforeEach (done) ->
                _viewModel = { item1: -> }

                # Here we are defining a stub to be returned instead of the real module
                # Can do this for some/all modules
                # modules that are not defined in the stubs will get loaded normally (ie jquery, ko etc)
                stubs =
                    'cs!dep1': sinon.stub().returns _viewModel

                createContext stubs, 'cs!dep2', (dep2) ->
                    _otherDep = new dep2()
                    done()

            it 'should do x', ->
                sinon.stub(_viewModel, item1).returns('something')
                ...

## How it works

The test runner in .NET land runs the tests.html in a headless browser (phantomjs) using the mocha-phantomjs plugin. The result is some output 
that is parsed and returned as a set of JavascriptTest objects. In the tests.html file first all of the test dependencies are loaded
up (mocha, chai, sinon, requirejs) after which the config.js is loaded. It uses the config you have set up to create the helper 'createContext' 
function and then loads up your test modules using requirejs, note that this does NOT use your configuration. We effectively have two requirejs
contexts, one for your tests, and one for your app. You load application modules using the 'createContext', which also allows you to stub out dependencies.

Note that 'createContext' undefines your module before loading it up again, which means you can be sure knowing that you will have a newly
constructed dependency in each run. As such it is designed to be used in the beforeEach test stage in Mocha. Note that sub-dependencies are not re-loaded, and
also due to the nature of the loading I think only 'pure' requirejs defined modules will work.

## License

MIT Licensed