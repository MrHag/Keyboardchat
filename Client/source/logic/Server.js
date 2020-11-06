class ServerManager {
  static getAvatarURL(avatarID) {
    return window.location.origin + '/avatar/' + avatarID;
  }
}

export default ServerManager;