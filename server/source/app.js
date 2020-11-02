const App = require('express')();
const HTTP = require('http').Server(App);
const SocketIO = require('socket.io')(HTTP);

const Path = '../public/';

App.get('/', (req, res) => {
  console.log("Required files / = ", req.url);
  res.sendFile("index.html", {root: Path});
});

App.get('/*', (req, res) => {
    console.log("Required files = ", req.url);
    res.sendFile(req.url, {root: Path});
});

const PORT = 5000;
HTTP.listen(PORT, () => {
  console.log(`Server listening on port ${PORT}`);
  onServerStart();
});

const dbUrl = 'mongodb://localhost:27017/chat';
const Mongoose = require('mongoose');
const UserModel = require('./models/User');

async function DBregisterUser(userModel) {
  return new Promise((resolve, reject) => {
    const isExist = DBUserExist({
      name: userModel.name,
    });

    if (isExist) {
      reject("This username is taken!");
    } else {
      resolve("User is registered!");
    }
  })
}

async function DBUserExist(userModel) {
  return new Promise((resolve, reject) => {
    UserModel.findOne({
      name: userModel.name,
    }, (err, doc) => {
      if (err) throw err;
      if (doc) {
        return resolve(true);
      } else {
        resolve(false);
      }
    });
  })
}

function startDB() {
  Mongoose.connect(dbUrl, {
    useNewUrlParser: true,
    useCreateIndex: true,
  });

  const newUser = new UserModel({
    id: 123,
    name: 'Someone',
  });

  Mongoose.connection.once('open', () => {
    console.log('Connection to the MongoDB was made!');
  }).on('error', (err) => {
    console.log('Couldn\'t make connetion to the data base!');
  });
}

const UserController = require('./controllers/User');
console.log("UserController = ", UserController);

function startSocketIO() {
  SocketIO.on('connection', (socket) => {
    console.log("Socket: New user connected!");
    
    socket.on('auth', (data) => UserController.authorization(socket, data));
    socket.on('registration', (data) => UserController.registration(socket, data));

    socket.on('disconnect', () => {
      console.log("Socket: Someone has disconnected");
    });
  });
}

function onServerStart() {
  startSocketIO();
  startDB();
}