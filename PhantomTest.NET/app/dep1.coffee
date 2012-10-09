define ['jquery', 'knockout'], ($, ko) ->
	class ViewModel
		constructor: ->
			@item1 = ko.observable 5
			@item2 = ko.computed =>
				@item1() * @item1()

		getData: (cb) ->
			ajax = $.getJSON 'http://www.urltosomedata.com.blah'
			ajax.done (data) ->
				cb null, data
			ajax.fail (err) ->
				cb err
