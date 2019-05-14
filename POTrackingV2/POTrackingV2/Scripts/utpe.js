/* for Initial Avatar */
let userAvatar, userName, userNameWords = '';

userName = document.getElementById("userName").textContent;
userNameWords = userName.split(" ");
userAvatar = userNameWords.reduce((accumulator, currentValue, currentIndex, array) => {
    if (currentIndex > 0 && currentIndex < 2) {
        return accumulator + currentValue.charAt(0);
    } else {
        return accumulator;
    }
}, userName.charAt(0));

//menu item selected
var path = window.location.pathname;
if (path.split('/').length > 2) {
    path = path.replace("/" + path.split('/')[2],"");
}

$('.main-menu-item a').each(function () {
    if ($(this).attr('href').toLowerCase().indexOf(path.toLowerCase()) > -1) {
        $(this).addClass("active");
    }
});

document.getElementById("userAvatar").textContent = userAvatar;
/* end of Initial Avatar */

/* for Toggle Notification */
function toggleNotification() {
    var element = document.getElementById("notificationContainer");
    element.classList.toggle("not-show");
}
/* end of Toggle Notification */

/* donut chart */
/* donut chart */