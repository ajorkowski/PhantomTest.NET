# If you want to run coffeescript tests in the browser
# You cannot do it direct from the filesystem, point IIS to the tests.html folder
define [], ->
	describe 'dep1', ->
		_viewModel = null

		beforeEach (done) ->
			# Methods that you not stub will get loaded as per normal
			createContext null, 'cs!dep1', (dep1) ->
				_viewModel = new dep1()
				done()

		describe 'item1', ->
			it.skip 'should equal 5 initially', ->
				# skipped test
				expect(_viewModel.item1()).to.equal 6

		describe 'item2', ->
			it 'should equal squared of item value', ->
				# failing test!
				_viewModel.item1 6
				expect(_viewModel.item2()).to.equal 25

		describe 'getData', (done) ->
			it 'should return the data\r\n it is sent', ->
				# use sinon to catch data requests
				server = sinon.fakeServer.create()
				server.respondWith 'GET', 'http://www.urltosomedata.com.blah', [200, { "Content-Type": "application/json" }, '3']

				callback = sinon.spy()
				_viewModel.getData callback
				server.respond()

				expect(callback.calledWith(null, 3)).to.be.ok
				server.restore()
				