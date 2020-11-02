const UserModel = require('../models/User');

module.exports = class UserController {
  static authorization(socket, data) {
    UserModel.findOne({
      name: data.name,
      password: data.password
    }, (err, doc) => {
      if (err) throw err;
      if (doc) {
        console.log("User found!");
        socket.emit('auth', {
          data: null,
          error: null
        });
      } else {
        socket.emit('auth', {
          data: null,
          error: "wrongNamePass",
        });
      }
    });
  }

  static registration(socket, data) {
    console.log("Registration data = ", data);
    UserModel.countDocuments({}, (err, count) => {
      const id = count + 1; // TODO: Maybe this is bad idea...
      const user = new UserModel({
        id,
        ...data
      });
      user.save(null, (err, doc) => {
        console.log('err = ', err);
        console.log('doc = ', doc);
        if (doc === undefined || err) {
          console.log("This name is alredy taken!")
          socket.emit('registration', {
            data: null,
            error: "nameExists"
          });
        } else {
          socket.emit('registration', {
            data: null,
            error: null
          });
        }
      });
    });
  }
};