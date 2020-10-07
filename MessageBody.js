module.exports =
class MessageBody {
    constructor(name, message, avatar) {
        this.name = name;
        this.message = message;
        this.avatar = avatar;
    }

    static fromJSON(json) {
        return new MessageBody(json.name, json.message, json.avatar);
    }
}