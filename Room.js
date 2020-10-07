module.exports =
    class room {

        constructor(name, password = "") {
            this.name = name;
            this.password = password;
            this.users = [];
        }

        AddUser(user)
        {

            for (var key in this.users)
            {
                var findUser = this.users[key];
                if (findUser.client == user.client)
                    return;
            }

            this.users.push(user);

        }

        RemoveUser(user) {

            for (var key in this.users) {
                var findUser = this.users[key];
                if (findUser.client == user.client) {
                    this.users.splice(key, 1);
                    return;
                }
                    
            }

        }

    }