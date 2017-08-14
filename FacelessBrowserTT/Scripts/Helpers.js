class History {
	constructor(maxPages) {
		this.previousPages = new Array();
		this.nextPages = new Array();
		this.currentPage = undefined;
		this.maxPages = maxPages;
	}

	switchPage(fromArr, toArr, forced) {
		const page = fromArr.pop();
		if (forced || page != undefined) {
			if (this.currentPage != undefined)
				toArr.push(this.currentPage);
			this.currentPage = page;
		}

		if (toArr.length > this.maxPages)
			toArr.shift();

		return page;
	}

	previousPage() {
		return this.switchPage(this.previousPages, this.nextPages, true);
	}

	nextPage() {
		return this.switchPage(this.nextPages, this.previousPages, false);
	}

	setCurrentPage(address) {
		this.previousPages.push(this.currentPage);
		if (this.previousPages.length > this.maxPages)
			this.previousPages.shift();

		this.currentPage = address;
		this.nextPages = [];
	}
}

function replaceContent(html) {
	//const doc = document.getElementById("content").contentWindow.document;
	//doc.open();
	//doc.write(html);
	//doc.close();

	$("*:not(.faceless-internal)").remove();

	// Can't append google's head...
	//const head = html.match(/<head[^>]*>([\s\S]*)<\/head>/i)[1];
	//const body = html.match(/<body[^>]*>([\s\S]*)<\/body>/i)[1];
	//$("head").append(head);
	//$("#content").html(body);

	$("#content").html(html);
	
}

function getFullUrl(url, isRelative) {
	if (url.search(/^https?:\/\//) === -1) {
		if (url.search(/^\/\//) !== -1)
			url = `http:${url}`;
		else if (isRelative)
			url = browserHistory.currentPage.replace(/\/$/, "") + "/" + url.replace(/^\//, "");
		else
			url = `http://${url}`;
	}

	return url;
}

