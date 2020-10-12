module.exports =
class responseBody {
    constructor(data, succ, error) {
        this.data = data;
        this.successful = succ;
        this.error = error;
    }   
}