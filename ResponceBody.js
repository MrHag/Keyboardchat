module.exports =
class ResponceBody {
    constructor(type, message, error) {
        this.type = type;
        this.message = message;
        this.error = error;
    }   
}