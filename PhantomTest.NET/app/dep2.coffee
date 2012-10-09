define ['cs!dep1'], (dep1) ->
	class OtherDep
		constructor: ->
			@viewModel = new dep1()

		findData: ->
			@viewModel.item1()
