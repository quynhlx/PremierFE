/*
 * upeek app version 3.1.7G 7/2/2015
 *
 * master branch
 *
 * update after ping
 */



jwplayer.key = 'l33PiUta/XdDcW+Etg4rUPibBw1qZApFA2EFFjZwgHw=';



var api = 'http://38.107.179.44/api/v2';
api = 'https://ws1.upeek.tv/api/v2';
var stream;
var replay = false;
var feed;
var intervalUpdateId;
var oldAppViewers;
var oldWebViewers = 0;
var stopped = false;
var log = {};
var pingId;
var firstPlay = false;

var tryReplay = 0;

/*
 * session storage (for web viewer)
 *
 */
var session;



$(document).ready(function(){

	/*
	 * api for test mode
	 *
	 */
	var dev = getParamValue('dev');
	if (dev !== '')
		api = dev;

    /*
    * Let's check the browser version
    */
    var isOpera = !!window.opera || navigator.userAgent.indexOf(' OPR/') >= 0;
    // Opera 8.0+ (UA detection to detect Blink/v8-powered Opera)
    var isFirefox = typeof InstallTrigger !== 'undefined';   // Firefox 1.0+
    var isSafari = Object.prototype.toString.call(window.HTMLElement).indexOf('Constructor') > 0;
    // At least Safari 3+: "[object HTMLElementConstructor]"
    var isChrome = !!window.chrome && !isOpera;              // Chrome 1+
    var isIE = /*@cc_on!@*/false || !!document.documentMode; // At least IE6
	
    var fullsid = getParamValue('streamid');
    if (fullsid === '') {
        return;
    }
    var sid = fullsid;
    var sidArr = sid.split('_');
    if (sidArr.length > 1) {
        //Diregard the SegmentID so the stream info can be looked up - GG
        sid=sidArr[0];
    }

    stream = new Stream(sid);

    if (isSafari) {
      //Using Safari
      //Make all HLS
      stream.streamFileH = 'http://wl.upeek.tv:8081/rslive/' + sid + '/playlist.m3u8';
      stream.streamFileR = 'http://wl.upeek.tv:8081/rslive/' + sid + '/playlist.m3u8';
      stream.replayFileH = 'http://la1.upeek.tv:1935/hist_upeek/mp4:'+fullsid+'.mp4/playlist.m3u8';
      stream.replayFileR = 'http://la1.upeek.tv:1935/hist_upeek/mp4:'+fullsid+'.mp4/playlist.m3u8';
    } else {
      //NOT using Safari
      //Allow RTMP
      stream.streamFileH = 'http://wl.upeek.tv:8081/rslive/' + sid + '/playlist.m3u8';
      stream.streamFileR = 'rtmp://wl.upeek.tv:1935/rslive/' + sid;
      stream.replayFileH = 'http://la1.upeek.tv:1935/hist_upeek/mp4:'+fullsid+'.mp4/playlist.m3u8';
      stream.replayFileR = 'rtmp://la1.upeek.tv:1935/hist_upeek/flv:'+fullsid;
    }

    /*
     * setup jwplayer
     */
	jwplayer('jw-player').setup({
            playlist: [{
              sources: [{ 
                file: stream.streamFileR
              },{ 
                file: stream.streamFileH
              }]
            }],
            title: "Watch Live",
            primary: 'flash',
            width: '100%',
            height: '100%',
            stretching: 'fill',
            androidhls: true,
            aspectratio: '9:16',
            wmode: 'transparent',
            controls: true,
            rtmp: {
              bufferlength: 7.5
            },
            autostart: true,
        });

    $.when(loadStreamInfo(sid)).then(function (resp, stt, jqXHR) {

        jwplayer().onReady(function (e) {
            jwplayer().load({
                            image: stream.streamCover,
                            sources: [{ 
            			  file: stream.streamFileH
        			  },{
            			  file: stream.streamFileR
        			}],
            }).onPlaylist(function () {
                $('.btn-play').html('start watching');
            }).onPlay(function () {
                togglePlay(true);
            }).onPause(function () {
                togglePlay(false);                
            }).onIdle(function () {
                togglePlay(false);
            }).onError(function () {
                tryReplay++;        
                var time_id = setInterval(function() {
                  if ((tryReplay < 20) && (stream.allowReplay > 0)) {
                    clearInterval(time_id);
                  } else {
                    jwplayer().stop();
                  }
                }, 500);
                jwplayer().load({
                            image: stream.streamCover,
        			sources: [{ 
            			  file: stream.replayFileR
        			  },{
            			  file: stream.replayFileH
                            }],
                }).onPlaylist(function () {
                    $('.btn-play').html('watch replay');
                }).onPlay(function () {
                    replay = true;
                    togglePlay(true);
                }).onPause(function () {
                    togglePlay(false);
                }).onIdle(function () {
                    togglePlay(false);
                }).onError(function () {
                    replay = false;
                    streamOver();
                }).onComplete(function () {
                    replay = false;
                    streamOver();
                });
            }).onComplete(function () {
                replay = false;
                streamOver();
            });
        });

        /*
         * update after 5s
         */
        intervalUpdateId = setInterval(updateInfo, 5000);
		
		/*
		 * ping to server to check anonymous still online
		 */
		pingId = setInterval(pingByAnonymous, 30000);
    });
    /*
     * show social share
     */
    $('.btn-share').click(function () {
        $(this).next().toggleClass('active');
    });

    /*
     * toggle jwplayer play
     */
    $('.btn-play').click(function () {
        jwplayer().play();
    });

    /*
     * facebook feed dialog
     */
    $('.share-facebook').click(function (e) {

        e.preventDefault();

        FB.ui(feed, function (resp) { });

    });

    /*
     * social share popup
     */
    $('.share').click(function (e) {

        e.preventDefault();

        var url = $(this).prop('href');

        if (url.indexOf('#') === (url.length - 1)) {
            return false;
        }

        var width = 500, height = 350;
        var top = screen.height / 2 - height / 2;
        var left = screen.width / 2 - width / 2;

        window.open(url, 'uPeek Share', 'top=' + top + ', left=' + left + ', width=' + width + ', height=' + height + ', toolbar=1, resizable=0');
    });
	
    /*
     * stop propagation link on stream over popup
     */
	$('.overlay-over').on('click', 'a', function(e) {
		e.preventDefault();
	});

    /*
     * support on touch device
     */
	$('.player').on('touchstart',function(){		
		$('.info').slideToggle(250);
	});
	
});


/*
 * @description: get value of parameter in query url
 * 
 * @param {paramName}: a string of parameter name
 * @returns: a string os parameter value
 */
function getParamValue(paramName) {
    if (paramName === undefined || paramName === '')
        return '';

    var query = window.location.search.trim().replace('?', '');
    if (query === '')
        return '';

    var arr = query.split('&');
    // if ($.isArray(arr)) return '';
    if (arr === undefined || arr === null || arr.length === 0)
        return '';

    for (var i = 0; i < arr.length; i++) {
        var param = arr[i].split('=');

        if (param[0].toLowerCase() === paramName.toLowerCase())
            return param[1];
    }

    return '';
}


/*
 * @description: stream information
 * 
 * 
 * 
 */
function Stream(streamId) {
    this.streamId = streamId;
    this.publishId = -1;
    this.user = '';
    this.title = '';
    this.category = '';
    this.totalShare = 0;
    this.totalLike = 0;
    this.totalView = 1;
    this.currentView = 1;
    this.shareLink = '';
    this.allowReplay = 0;
    this.coverImage = '';
    this.streamFileH = '';
    this.streamFileR = '';
    this.replayFileH = '';
    this.replayFileR = '';
}


/*
 * @description: create a tricky url to delete ajax cached
 * 
 * @returns: a string has format t=123456789
 */
function trickyUrl() {
    return 't=' + $.now();
}


/*
 * @description: load stream information from webservice
 * 
 * @param {streamId}: a string of stream id
 * @returns: promise object of ajax call
 */
function loadStreamInfo(streamId) {

    /*
     * url format
     *
     * http://ws1.upeek.tv/api/publishing/getbyname?name=7ec85778-6012-45fa-bdd2-bbc619b8f1ab&t=1432781004931
     *
     */
    
    var url = api + '/publishing/getbyname?name=' + streamId + '&' + trickyUrl();    
    var req = $.ajax({ url: url, timeout: 15000 });
	
	log.loadStreamInfoUrl = url;

    req.fail(function (jqXHR, stt, err) {

		log.loadStreamInfoXHR = jqXHR;
		log.loadStreamInfoErr = err;
		
        $('.btn-play').html('Error occured');
    });

    req.done(function (resp, stt, jqXHR) {

        var data = resp.Data;
        log.loadStreamInfoResponse = resp;

        if (data === undefined || data === null) {

            $('.btn-play').html('try again');

            return;
        }
       
        data = formatData(data);

        var user = data.Username;
        var profileImage = data.ProfileImage;

        var title = data.Title;
        var loca = data.LocationName;
        var cate = data.Category;

        var share = data.TotalShare;
        var like = data.TotalLike;
        var currView = data.CurrentPlay;
        var totalView = data.TotalPlay;
	 var shareLink = data.Link;
        var allowReplay = data.AllowReplay
		
		
        stream.publishId = data.PublishID;        
        stream.streamCover = data.StreamFullImageUrl;
		
		stream.totalLike = like;
		stream.totalShare = share;
		stream.totalView = totalView;
              stream.allowReplay = allowReplay;
		
		if (currView === undefined || currView === 0)
			currView = 1;
		stream.currentView = currView;
	

        updateUser(user, profileImage);
        updateDetail(title, loca, cate);
        updateCount(stream.totalShare, stream.totalLike, stream.currentView);

        /*
         * facebook feed dialog setup
         */
        feed = {
            method: 'feed',
            display: 'popup',
            link: data.Link,
            caption: cate,
            picture: stream.streamCover,
            description: title,
            name: user
        };

        
        /*
         * update social share link
         */
        var fbShare = 'http://www.facebook.com/sharer.php?u=' + data.Link;
        var twShare = 'http://twitter.com/share?url=' + encodeURI(shareLink) + '&text=' + title;
        var gpShare = 'https://plus.google.com/share?url=' + shareLink;
        var emShare = 'mailto:?subject=Watch%live%on%uPeek?body=' + shareLink;

        $('.share-facebook').prop('href', fbShare);
        $('.share-twitter').prop('href', twShare);
        $('.share-gplus').prop('href', gpShare);
        $('.share-email').prop('href', emShare);

    });
     

    return req;
}

/*
 * @description: format undefined data
 *
 * @param data: an object to format
 * @returns: formated data. return empty string if data is undefined
 */
function formatData(data) {

    if (typeof data === 'object') {
        $.each(data, function (key, value) {
            if (value === undefined || value === null || value === '')
                data[key] = '';
        });

        return data;
    }


    if (data === undefined || data === null || data === '')
        return ' ';

    return data;
}


/*
 * @description: update user to UI
 * 
 * @param user: a string of username
 * @param image: a string of user profile picture
 * 
 */
function updateUser (username, image) {
    var $user = $('.info-user');

    $user.children('div').first().children('img').prop('src', image);
    $user.children('div').last().text(username);
}


/*
 * @description: update user detail to UI
 * 
 * @param title: a string of stream title
 * @param loca: a string of user location
 * @param cate: a string of stream categoty
 * 
 */
function updateDetail(title, loca, cate) {
    var $detail = $('.info-detail');

    $detail.children('.detail-title').text(title);
    $detail.children('.detail-location').text(loca);
    $detail.children('.detail-category').text(cate);
}


/*
 * @description: update social couting to UI
 * 
 * @param share: total share of this stream in mobile app
 * @param like: total like of this tream in mobile app
 * @param view: current viewer of this stream in mobile app
 */
function updateCount(share, like, view) {
    var $count = $('.info-count');

    // make sure that it is a number
	if (isNaN(share)) share = 0;
	if (isNaN(like)) like = 0;
	if (isNaN(view) || view === 0) view = 1;
	
	
    $count.children('.count-share').find('span').text(share);
    $count.children('.count-like').find('span').text(like);
	$count.children('.count-view').find('span').text(view);
}

/*
 * @description: call this function when stream is over or play completed
 * 
 * clear update interval
 * clear ping interval
 */
function streamOver() {

    if (stopped) {
        return;
    }
    //Check to see if stream is currently in replay mode
    if (replay) {
        updateCount(stream.totalShare, stream.totalLike, stream.totalView); // update totalview instead current view
        return;
    }

    jwplayer().play(false);
	
    stopped = true;

	/*
	 * clear anonymous
	 *
	 */
	 if (session !== undefined && typeof session === 'object')
		session.clear();

    $('.main').removeClass('playing'); // for show info banner
    $('.overlay').fadeIn(); // show streamover overlay
    $('.overlay-button').addClass('hidden'); // hide 'start watching button'
    $('.overlay-over').removeClass('hidden'); // and show 3 button 'follow, like and chat' like app
    
    updateCount(stream.totalShare, stream.totalLike, stream.totalView); // update totalview instead current view

    /*
     * cancel update interval
     */
    if (intervalUpdateId !== undefined && !isNaN(intervalUpdateId)) {
		clearInterval(intervalUpdateId);
		//console.log('update cancelled');
    }

    /*
     * cancel anonymous web ping interval
     */
    if (pingId !== undefined && !isNaN(pingId)) {
        clearInterval(pingId);
    }
        
}

/*
 * @description: toggle player state (pause - play)
 *              if state = true  --> play  --> hide overlay
 *              if state = false --> pause --> show overlay
 *
 * @param isPlay: a boolean of play state
 */
function togglePlay(isPlay) {
    if (isPlay) {
        $('.overlay').fadeOut();
        $('.main').addClass('playing');
    } else {
        $('.overlay').fadeIn();
        $('.main').removeClass('playing');
    }
}


/*
 * @description: update info + check stream over every 5s
 * 
 */
function updateInfo() {

    if (stopped) {

        if (intervalUpdateId !== undefined && !isNaN(intervalUpdateId))
            clearInterval(intervalUpdateId);

        log.stopped = stopped;

        return;
    }

    /*
     * format url
     * streamid mean publishid
     * http://ws1.upeek.tv/api/publishing/checkstreamlive3?streamid=1234&t=123456789
     */
    var url = api + '/publishing/checkstreamlive3?streamid=' + stream.publishId + '&' + trickyUrl();
    var req = $.ajax({ url: url, timeout: 5000 });

    log.updateUrl = url;

    req.fail(function (jqXHR, stt, err) {
        log.updateInfoErr = jqXHR;
    });

    req.done(function (resp, stt, jqXHR) {
        
        var data = resp.Data;
        log.updateInfoResponse = resp;

        if (data === undefined || data === null)
            return;
		
		/*
         * update stream count in case of stream over
         *
		if (isNaN(data.TotalWebView))
			data.TotalWebView = 0;
		*/
		
		if (!isNaN(data.TotalPlay) && data.TotalPlay > 0)
			stream.totalView = data.TotalPlay;
		
		if (!isNaN(data.TotalLike) && data.TotalLike > 0)
			stream.totalLike = data.TotalLike;
		
		if (!isNaN(data.TotalShare) && data.TotalShare > 0)
			stream.totalShare = data.TotalShare;
        
        if (!data.StreamALive) {
            streamOver();
            return;
        }
		
		
		if (!isNaN(data.CurrentPlay))		 
			stream.currentView = data.CurrentPlay === 0 ? 1 : data.CurrentPlay;
		
        updateCount(stream.totalShare, stream.totalLike, stream.currentView);
		
    });

}

/*
 * @description: ping to server to check web viewer still on session
 *
 */
function pingByAnonymous() {

    // check for session storage support
    session = window.sessionStorage;
    if (session === undefined || typeof session !== 'object')
        return;

    /*
     * if session storage support then add anonymous web viewer
     *
     */
    // //console.log('anonymous', session.anonymous);
    if (session.anonymous === undefined) { // first view


        /*
         * post new request to add anonymous web view
         */
		if (stream.publishId === -1) {
			return;
		}
        var anonymousUrl = api + '/webview/add';
        var anonymousData = { ID: 0, StreamID: stream.publishId };
        var anonymousReq = $.ajax({ url: anonymousUrl, data: anonymousData, method: 'post' });  

        anonymousReq.fail(function (jqXHR, err, sttText) {
            log.anonymousErr = jqXHR;
        }).done(function (resp, stt, jqXHR) {
            log.anonymousResponse = resp;

            /*
             * get anonymous id to ping
             */
            var anonymousId = +resp.Data;

            if (isNaN(anonymousId) || anonymousId < 0) {
                return;
            }

            /*
             * save anonymous to session storage
             */
            var anonymous = { anonymousId: anonymousId };
            session.anonymous = JSON.stringify(anonymous); // convert object to string
			
			/*
			 * update current view immediatelly
			 */
			 /*
			if (isNaN(stream.currentView)) 
				stream.currentView = 1;
			else
				stream.currentView += 1;
			*/
			// updateCount(stream.totalShare, stream.totalLike, stream.currentView);
			updateInfo();

        });

    } else { // anonymous viewer still on session
        log.anonymous = JSON.parse(session.anonymous);
		
		var anonymous = JSON.parse(session.anonymous);
        /*
        * ping format url
        *
        * id = anonymous id response
        * http://ws1.upeek.tv/webview/lastalive?id=123&t=123456798
        */
        var pingurl = api + '/webview/lastalive?id=' + anonymous.anonymousId + '&' + trickyUrl(); // final
        var pingreq = $.ajax({ url: pingurl, timeout: 30000 });

        pingreq.done(function (resp, stt, jqXHR) {
            log.pingSuccess = resp;
        }).fail(function (jqXHR, stt, err) {
            log.pingErr = jqXHR;
        });
    }
}