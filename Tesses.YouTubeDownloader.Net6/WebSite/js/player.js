const playPauseBtn = document.querySelector(".play-pause-btn")
const theaterBtn = document.querySelector(".theater-btn")
const fullScreenBtn = document.querySelector(".full-screen-btn")
const miniPlayerBtn = document.querySelector(".mini-player-btn")
const muteBtn = document.querySelector(".mute-btn")
const captionsBtn = document.querySelector(".captions-btn")
const speedBtn = document.querySelector(".speed-btn")
const currentTimeElem = document.querySelector(".current-time")
const totalTimeElem = document.querySelector(".total-time")
const thumbnailImg = document.querySelector(".thumbnail-img")
const volumeSlider = document.querySelector(".volume-slider")
const videoContainer = document.querySelector(".video-container")
const timelineContainer = document.querySelector(".timeline-container")
const video = document.querySelector("video")
const mux=document.getElementById("Mux");
const premux=document.getElementById("PreMuxed");
 const audio=document.getElementById("Audio");
 const resbtn =document.querySelector(".res-btn");
 const dlbtn =document.querySelector(".download-btn");
 
 const videoTitle = document.getElementById("video-title");
 const videoViews = document.getElementById("video-views");
 const videoLikes = document.getElementById("video-likes");
 const videoDislikes = document.getElementById("video-dislikes");
 const videoUploadDate = document.getElementById("video-date");
 const videoChannelIcon = document.getElementById("video-channel-icon");
 const videoChannelTitle = document.getElementById("video-channel-title");
 const videoDescription = document.getElementById("video-description");
var res=1;
var id=getid();

function getfilename(id,res)
{
  return `../api/Storage/VideoRes/${res}/${id}`;
}



function reviver(value) {
  
  if (typeof value === "string") {
    return new Date(value.substring(0,value.indexOf("T")));
  }

  return value;
}
function getid()
{
  const params = new Proxy(new URLSearchParams(window.location.search), {
    get: (searchParams, prop) => searchParams.get(prop),
  });
  
  mux.href=getfilename(params.v,0);
   premux.href=getfilename(params.v,1);  
  audio.href=getfilename(params.v,2);
  video.src=getfilename(params.v,1);
  var tytd=new TYTD(window.location,1);
  tytd.getvideoinfo(params.v,(e)=>{
      videoTitle.innerText = e.Title;
      videoViews.innerText = num2unit(e.Views);
      if(e.Likes>0)
      {
        videoLikes.innerText = num2unit(e.Likes);
      }
      if(e.Dislikes>0)
      {
        videoDislikes.innerText=num2unit(e.Dislikes);
      }
      videoChannelTitle.innerText=e.AuthorTitle;
      videoDescription.innerText = e.Description;
      videoChannelIcon.src=`../api/Storage/File/Thumbnails/${e.AuthorChannelId}/900x900.jpg`
      var date=reviver(e.UploadDate);
      
let formattedDate = new Intl.DateTimeFormat("en-US", {
  year: "numeric",
  month: "short",
  day: "2-digit",
}).format(date);
videoUploadDate.innerText=formattedDate;
  })
  return params.v;
 
}
resbtn.addEventListener("click",()=>{
  res++;
  if(res >2)
  {
    res=0;
  }
  switch(res)
  {
    case 0:
      resbtn.innerText="Mux";

      break;
    case 1:
      resbtn.innerText="PreMuxed";
      break;
    case 2:
      resbtn.innerText="Audio";
      break;
  }
  video.src=getfilename(id,res);

})
dlbtn.addEventListener("click",()=>{
    switch(res)
    {
       case 0:
        mux.click();
       break;
       case 1:
         premux.click();
         break;
        case 2:
          audio.click();
          break;
    }
});
document.addEventListener("keydown", e => {
  const tagName = document.activeElement.tagName.toLowerCase()

  if (tagName === "input") return

  switch (e.key.toLowerCase()) {
    case " ":
      if (tagName === "button") return
    case "k":
      togglePlay()
      break
    case "f":
      toggleFullScreenMode()
      break
    case "t":
      toggleTheaterMode()
      break
    case "i":
      toggleMiniPlayerMode()
      break
    case "m":
      toggleMute()
      break
    case "arrowleft":
    case "j":
      skip(-5)
      break
    case "arrowright":
    case "l":
      skip(5)
      break
    case "c":
      toggleCaptions()
      break
  }
})

// Timeline
timelineContainer.addEventListener("mousemove", handleTimelineUpdate)
timelineContainer.addEventListener("mousedown", toggleScrubbing)
document.addEventListener("mouseup", e => {
  if (isScrubbing) toggleScrubbing(e)
})
document.addEventListener("mousemove", e => {
  if (isScrubbing) handleTimelineUpdate(e)
})

let isScrubbing = false
let wasPaused
function toggleScrubbing(e) {
  const rect = timelineContainer.getBoundingClientRect()
  const percent = Math.min(Math.max(0, e.x - rect.x), rect.width) / rect.width
  isScrubbing = (e.buttons & 1) === 1
  videoContainer.classList.toggle("scrubbing", isScrubbing)
  if (isScrubbing) {
    wasPaused = video.paused
    video.pause()
  } else {
    video.currentTime = percent * video.duration
    if (!wasPaused) video.play()
  }

  handleTimelineUpdate(e)
}

function handleTimelineUpdate(e) {
  const rect = timelineContainer.getBoundingClientRect()
  const percent = Math.min(Math.max(0, e.x - rect.x), rect.width) / rect.width
  const previewImgNumber = Math.max(
    1,
    Math.floor((percent * video.duration) / 10)
  )
 
  timelineContainer.style.setProperty("--preview-position", percent)

  if (isScrubbing) {
    e.preventDefault()
    thumbnailImg.src = previewImgSrc
    timelineContainer.style.setProperty("--progress-position", percent)
  }
}

// Playback Speed
speedBtn.addEventListener("click", changePlaybackSpeed)

function changePlaybackSpeed() {
  let newPlaybackRate = video.playbackRate + 0.25
  if (newPlaybackRate > 2) newPlaybackRate = 0.25
  video.playbackRate = newPlaybackRate
  speedBtn.textContent = `${newPlaybackRate}x`
}

// Captions


// Duration
video.addEventListener("loadeddata", () => {
  totalTimeElem.textContent = formatDuration(video.duration)
})

video.addEventListener("timeupdate", () => {
  currentTimeElem.textContent = formatDuration(video.currentTime)
  const percent = video.currentTime / video.duration
  timelineContainer.style.setProperty("--progress-position", percent)
})

const leadingZeroFormatter = new Intl.NumberFormat(undefined, {
  minimumIntegerDigits: 2,
})
function formatDuration(time) {
  const seconds = Math.floor(time % 60)
  const minutes = Math.floor(time / 60) % 60
  const hours = Math.floor(time / 3600)
  if (hours === 0) {
    return `${minutes}:${leadingZeroFormatter.format(seconds)}`
  } else {
    return `${hours}:${leadingZeroFormatter.format(
      minutes
    )}:${leadingZeroFormatter.format(seconds)}`
  }
}

function skip(duration) {
  video.currentTime += duration
}

// Volume
muteBtn.addEventListener("click", toggleMute)
volumeSlider.addEventListener("input", e => {
  video.volume = e.target.value
  video.muted = e.target.value === 0
})

function toggleMute() {
  video.muted = !video.muted
}

video.addEventListener("volumechange", () => {
  volumeSlider.value = video.volume
  let volumeLevel
  if (video.muted || video.volume === 0) {
    volumeSlider.value = 0
    volumeLevel = "muted"
  } else if (video.volume >= 0.5) {
    volumeLevel = "high"
  } else {
    volumeLevel = "low"
  }

  videoContainer.dataset.volumeLevel = volumeLevel
})

// View Modes
theaterBtn.addEventListener("click", toggleTheaterMode)
fullScreenBtn.addEventListener("click", toggleFullScreenMode)
miniPlayerBtn.addEventListener("click", toggleMiniPlayerMode)

function toggleTheaterMode() {
  videoContainer.classList.toggle("theater")
}

function toggleFullScreenMode() {
  if (document.fullscreenElement == null) {
    videoContainer.requestFullscreen()
  } else {
    document.exitFullscreen()
  }
}

function toggleMiniPlayerMode() {
  if (videoContainer.classList.contains("mini-player")) {
    document.exitPictureInPicture()
  } else {
    video.requestPictureInPicture()
  }
}

document.addEventListener("fullscreenchange", () => {
  videoContainer.classList.toggle("full-screen", document.fullscreenElement)
})

video.addEventListener("enterpictureinpicture", () => {
  videoContainer.classList.add("mini-player")
})

video.addEventListener("leavepictureinpicture", () => {
  videoContainer.classList.remove("mini-player")
})

// Play/Pause
playPauseBtn.addEventListener("click", togglePlay)
video.addEventListener("click", togglePlay)

function togglePlay() {
  video.paused ? video.play() : video.pause()
}

video.addEventListener("play", () => {
  videoContainer.classList.remove("paused")
})

video.addEventListener("pause", () => {
  videoContainer.classList.add("paused")
})
