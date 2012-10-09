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

		describe 'findData', ->
			it 'should call item1 from dep1', ->
				spy = sinon.spy _viewModel, 'item1'

				_otherDep.findData()

				expect(spy.called).to.be.ok