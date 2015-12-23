chrome.tabs.onUpdated.addListener(function(tabId, changeInfo, tab) {
   checkForValidUrl(tabId, tab);
}); 

chrome.tabs.onActivated.addListener(function(activeInfo) {
  // how to fetch tab url using activeInfo.tabid
  chrome.tabs.get(activeInfo.tabId, function(tab){
	 checkForValidUrl(tab.tabId, tab);
  });
}); 

function checkForValidUrl(tabId, tab) {
// If the tabs url starts with "http://specificsite.com"...
if (tab.url.indexOf('https://stackoverflow.com/questions/') == 0) {
// ... show the page action.
chrome.pageAction.show(tabId);
}
};