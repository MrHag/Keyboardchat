import React, { useEffect, useRef, useState, useContext } from 'react';
import PropTypes from 'prop-types';

import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import * as FontAwesomeIcons from '@fortawesome/free-solid-svg-icons';

import { Input, RoomItem, Button, LabelButton, IconButton } from 'components';
import { Socket, UserData } from 'logic';

import './RoomPanel.scss';

try {
  var fake_rooms = null;// require('fake_data/fake.json').rooms;
} catch (err) { }

const RoomList = ({ inRoom, rooms, joinRoom, leaveRoom }) => {
  const listRef = useRef();

  const activeRooms = rooms.filter((room) => room.compare(inRoom));
  const list = rooms.filter((room) => !room.compare(inRoom));
  if (activeRooms.length != 0) {
    list.unshift(...activeRooms);
  }

  return (
    <div ref={listRef} className="room-panel__list">
      {
        list.map((room, index) => {
          const isActive = room.compare(inRoom);
          return (
            <RoomItem
              key={`${room.id}_${index}`}
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

const RoomPanel = ({ onCreateRoom, onRoomLeave }) => {
  const [rooms, setRooms] = useState((fake_rooms) || []);
  const [searchQuery, setSearchQuery] = useState('');
  const [inRoom, setInRoom] = useState(UserData.inRoom);

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
    console.log('Trying to leave room: ', roomid);
    Socket.emit('leaveRoom', {
      id: roomid,
    });
    onRoomLeave();
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
    Socket.emit('getRooms', { room: null }); // TODO: Uncomment this in
  };

  const removeSocketListeners = () => {
    Socket.removeEventListener('getRooms', socketGetrooms);
    Socket.removeEventListener('joinRoom', socketJoinroom);
    Socket.removeEventListener('roomChange', socketRoomchange);
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
};

export default RoomPanel;
