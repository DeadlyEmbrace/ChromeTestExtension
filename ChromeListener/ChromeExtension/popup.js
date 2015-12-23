//See here to pass data to c#
//https://stackoverflow.com/questions/13910906/how-can-i-communicate-with-chromechrome-extension-using-c


var rgx = /^\bhttps:\/\/stackoverflow\.com\/questions\/\b([0-9]*)/;

$(document).ready(function() {
	chrome.tabs.getSelected(null, function(tab) {
		var matchVal = tab.url.match(rgx);
		if(matchVal.length == 0){
			qValue = "Unknown";
		} else {
			var qValue = matchVal[0].replace("https://stackoverflow.com/questions/","");
		}
		$("#TxtQuestionId").text(qValue);
		
		$.ajax({
		    url: "http://localhost:60024/question/" + qValue,
		    method: "POST",
			dataType: 'json',
		    success: function(data){
				$("#TxtResult").text("Yes - " + data["Message"]);
			},
			error: function(data){
				$("#TxtResult").text("No - "  + data["Message"]);
			}
		});
		
		
	});
});