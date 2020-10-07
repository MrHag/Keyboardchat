module.exports =
    class user {
        constructor(client, name = "", room = null, Auth = false) {
            this.client = client;
            this.name = name;
            this.room = room;
            this.Auth = Auth;
        }
    }