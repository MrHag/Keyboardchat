class Room {
  constructor(id, name, haspass) {
    this.id = id;
    this.name = name;
    this.haspass = haspass;
  }

  static fromJSON(json) {
    return new Room(
      json.id,
      json.room,
      json.haspass,
    );
  }

  compare(room) {
    return (this.id === room.id) && (this.name === room.name);
  }
}

class Message {
  constructor(author, content, roomId, date) {
    this.roomdId = roomId;
    this.author = author;
    this.content = content;
    this.date = date;
  }
}

class User {
  constructor(id, name, avatar, avatarHash) {
    this.id = id;
    this.name = name;
    this.avatar = avatar;
    this.avatarHash = avatarHash;
  }

  static fromJSON(json) {
    return new User(
      json.id,
      json.name,
      // json.avatar,
      "Hello",
      json.avatarHash,
    );
  }
}

export { Room, Message, User };

const UserData = {
  user: null,

  inRoom: new Room(undefined, 'globals'),
  setInRoomJSON(json) {
    this.inRoom = Room.fromJSON(json);
  },
  setInRoom(room) {
    this.inRoom = room;
  },

  existingRooms: {
    rooms: [],

    addRoomJSON(json) {
      this.rooms.push(Room.fromJSON(json));
    },

    setRoomsJSON(json) {
      this.rooms = json.data.map((room) => Room.fromJSON(room));
    },
  },

  toDefault: function() {
    this.user = null;
    this.inRoom = new Room(undefined, 'globals');
    this.existingRooms.rooms = [];
    this.cache.rooms = [];
  },

  cache: {
    rooms: [],
  },
};

UserData.toDefault();

export default UserData;
