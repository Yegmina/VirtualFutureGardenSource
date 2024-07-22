mergeInto(LibraryManager.library, {
  GetUserName: function() {
  //  alert('GetUserName function called');

    var urlParams = new URLSearchParams(window.location.search);
    var userName = urlParams.get('username');
    var token = urlParams.get('token');

  //  alert('Received URL params - username: ' + userName + ' token: ' + token);

    if (userName && token) {
      try {
       // alert('Attempting to compute hash for userName with secret key');
        var hashedUserName = sha256(userName + 'uniqueSecretKey');
      //  alert('Computed hash for username: ' + hashedUserName);

        if (hashedUserName === token) {
       //   alert('Token is valid, returning userName');
          var buffer = _malloc(userName.length + 1);
          writeAsciiToMemory(userName, buffer);
          return buffer;
        } else {
        //  alert('Token is invalid');
        }
      } catch (error) {
      //  alert('Error computing hash: ' + error);
      }
    } else {
      if (!userName) {
       // alert('Missing username');
      }
      if (!token) {
      //  alert('Missing token');
      }
    }

    //alert('Invalid or expired token');
    window.location.href = 'http://www.hotgames.dn.ua/';
    var defaultResponse = "User not available";
    var buffer = _malloc(defaultResponse.length + 1);
    writeAsciiToMemory(defaultResponse, buffer);
    return buffer;
  }
});

// Простая реализация SHA-256 с использованием CryptoJS
function sha256(message) {
//  alert('sha256 function called with message: ' + message);
  var hash = CryptoJS.SHA256(message).toString(CryptoJS.enc.Hex);
  //alert('SHA-256 hash computed: ' + hash);
  return hash;
}
