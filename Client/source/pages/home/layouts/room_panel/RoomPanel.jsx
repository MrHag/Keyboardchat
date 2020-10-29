import React, { useEffect, useRef, useState } from 'react';
import PropTypes from 'prop-types';

import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import * as FontAwesomeIcons from '@fortawesome/free-solid-svg-icons';

import { Input, RoomItem, LabelButton, IconButton } from 'components';
import { Socket, UserData, Room } from 'logic';

import './RoomPanel.scss';

const RoomList = ({ inRoom, rooms, joinRoom, leaveRoom }) => {
  const listRef = useRef();

  console.log("RoomList.InRoomObject = ", inRoom);

  const activeRooms = rooms.filter((room) => room.compare(inRoom));
  const list = rooms.filter((room) => !room.compare(inRoom));
  if (activeRooms.length !== 0) {
    list.unshift(...activeRooms);
  }

  return (
    <div ref={listRef} className="room-panel__list">
      {
        list.map((room, index) => {
          const isActive = room.compare(inRoom);
          return (
            <RoomItem
              key={`${room.id}`}
              roomData={room}
              active={isActive}
              onRoomJoin={joinRoom}
              onRoomLeave={() => leaveRoom(room.id)}
            />
          );
        })
      }
    </div>
  );
};

RoomList.propTypes = {
  inRoom: PropTypes.instanceOf(Room).isRequired,
  rooms: PropTypes.arrayOf(PropTypes.object).isRequired,
  joinRoom: PropTypes.func.isRequired,
  leaveRoom: PropTypes.func.isRequired,
};

const RoomPanel = ({ onCreateRoom, onRoomLeave }) => {
  const [rooms, setRooms] = useState([]);
  const [searchQuery, setSearchQuery] = useState('');
  const [_, setInRoom] = useState(UserData.inRoom);

  const joinRoom = (room, password) => {
    console.log('Trying to join room! RoomData = ', room);
    const request = {
      id: room.id,
      password,
    };
    Socket.emit('joinRoom', request);
  };

  const socketJoinroom = (data) => {
    console.log('socketJoinroom data = ', data);
    if (data.successful) {
      console.log("You've joined to room = ", data.data);
      UserData.setInRoomJSON(data.data);
      setInRoom(UserData.inRoom);
    } else {
      console.error("Can't joint room!", data);
    }
  };

  const leaveRoom = (roomid) => {
    console.log('Trying to leave roomId: ', roomid);
    Socket.emit('leaveRoom', {
      id: roomid,
    });
    onRoomLeave();
  };

  const socketRoomLeave = (data) => {
    console.log("ON ROOM LEAVE RESPONSE: ", data);
  };

  const socketGetrooms = (data) => {
    console.log('Getrooms data = ', data);
    UserData.existingRooms.setRoomsJSON(data);
    console.log('UserData.existingRooms.rooms = ', UserData.existingRooms.rooms);
    setRooms(UserData.existingRooms.rooms);
  };

  const socketRoomchange = (data) => {
    console.warn('SocketRoomchange...');
    Socket.emit('getRooms', { room: null });
  };

  const initSocketsListeners = () => {
    Socket.on('getRooms', socketGetrooms);
    Socket.on('roomChange', socketRoomchange);
    Socket.on('joinRoom', socketJoinroom);
    Socket.on('leaveRoom', socketRoomLeave);
    Socket.emit('getRooms', { room: null }); // TODO: Uncomment this in
  };

  const removeSocketListeners = () => {
    Socket.removeEventListener('getRooms', socketGetrooms);
    Socket.removeEventListener('joinRoom', socketJoinroom);
    Socket.removeEventListener('roomChange', socketRoomchange);
    Socket.removeEventListener('leaveRoom', socketRoomLeave);
  };

  useEffect(() => {
    initSocketsListeners();
    return removeSocketListeners;
  }, []);

  const onClearSearchBtn = () => {
    setSearchQuery('');
    Socket.emit('getRooms', { room: null });
  };

  const onSearchChange = (e) => {
    const name = (e.target.value !== '') ? e.target.value : null;
    Socket.emit('getRooms', { room: name });
    setSearchQuery(e.target.value);
  };

  const searchBtn = (
    <IconButton
      disabled={searchQuery === ''}
      onClick={onClearSearchBtn}
    >
      <FontAwesomeIcon icon={FontAwesomeIcons.faTrash} />
    </IconButton>
  );

  const search = (
    <>
      <Input
        variant="round"
        className="room-panel__search"
        placeholder="Room search"
        value={searchQuery}
        onChange={onSearchChange}
        button={searchBtn}
      />
    </>
  );

  return (
    <div className="room-panel">
      {search}
      <LabelButton
        className="room-panel__add-btn"
        label="Rooms"
        button={(
          <IconButton
            onClick={onCreateRoom}
          >
            <FontAwesomeIcon icon={FontAwesomeIcons.faPlus} />
          </IconButton>
        )}
      />
      <RoomList inRoom={UserData.inRoom} rooms={rooms} joinRoom={joinRoom} leaveRoom={leaveRoom} />
    </div>
  );
};

RoomPanel.propTypes = {
  onCreateRoom: PropTypes.func.isRequired,
  onRoomLeave: PropTypes.func.isRequired,
};

export default RoomPanel;
