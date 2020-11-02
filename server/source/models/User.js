const Mongoose = require('mongoose');
const Schema = Mongoose.Schema;

const UserSchema = new Schema({
  id: { type: Number, unique: true },
  name: { type: String, unique: true },
  password: String
});

const UserModel = Mongoose.model('User', UserSchema);

module.exports = UserModel;