let browserHistory;

function loadPage(address, saveToHistory) {
	console.log("Try to load " + address);
	setLoadingImage();

	$.ajax({
		url: "/Home/GetPage",
		data: {address: address},
		success: function (data, textStatus) {
			if (saveToHistory)
				browserHistory.setCurrentPage(address);

			replaceContent(data);
			$("#address-field").trigger("blur");
			$("a").click(goByLink);
		},
		error: function(r, e) {
			if (address.search(/^https:\/\/www\.google\.ru\/search\?q=/) === -1)
				loadPage("https://www.google.ru/search?q=" + address.replace(/https?:\/\//, ""), saveToHistory);
			else {
				loadHomePage(`error: ${e}`);
				$("#address-field").focus();
			}
		}
	});

	return true;
}

function goBack() {
	const page = browserHistory.previousPage();
	if (page != undefined) {
		$("#address-field").val(page);
		loadPage(page, false);
	} else {
		loadHomePage();
	}
}

function goForward() {
	const page = browserHistory.nextPage();
	if (page != undefined) {
		$("#address-field").val(page);
		loadPage(page, false);

	}
}

function proceed() {
	const address = $("#address-field").val();
	if (address != "")
		loadPage(getFullUrl(address, false), true);
	else
		loadHomePage("Enter a valid url");

	//$("#content").html(address + ":: " + b);
}

function reload() {
	loadPage(browserHistory.currentPage, false);
}

function goByLink(e) {
	const address = e.target.getAttribute("href");
	loadPage(getFullUrl(address, true), true);
	return false;
}

function loadHomePage(text) {
	if (text == null)
		text =
			"Welcome to the Faceless browser! <br/>" +
			"You can start browsing by typing a text to the address text field above";
	//<link type="text/css" rel="Stylesheet" href="../../Content/Site.css" />
	replaceContent(`
		<html>\
		<head>\
			\
		</head>\
		<body>\
			<div class='homePage'>${text}</div>\
		</body>\
		</html>`);
	//"<iframe id='ifr' src=''></iframe> "
}

function setLoadingImage() {
	loadHomePage("<img src='../../Content/Images/loading-icon.gif' />");
}

$(document).ready(function () {
	$("#go-back-btn").click(goBack);
	$("#go-forward-btn").click(goForward);
	$("#reload-page-btn").click(reload);
	$("#proceed-btn").click(proceed);

	// proceed() on Enter when focus is on #address-field
	document.getElementById("address-field").onkeydown = function (e) {
		if (e.keyCode === 13)
			proceed();
	};

	let showTopPanel = function (e) {
		$("#top-panel").stop().animate({
			margin: "0px auto"
		}, 250);
	}

	let hideTopPanel = function (e) {
		// Don't hide if we are on home page
		if (browserHistory.currentPage != undefined &&
			!$("#address-field").is(":focus"))
			$("#top-panel").stop().animate({
				margin: "-35px auto"
			}, 250);
	}

	$("#address-field").focus(showTopPanel);
	$("#address-field").focusout(hideTopPanel);
	$("#top-panel").mouseenter(showTopPanel);
	$("#top-panel").mouseleave(hideTopPanel);

	$("*").addClass("faceless-internal");

	browserHistory = new History(4);

	loadHomePage();

	//loadPage("ru.wikipedia.org/wiki/Ди_Каприо,_Леонардо");
	
});