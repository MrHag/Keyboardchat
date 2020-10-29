import React, { useState, useEffect } from 'react';
import { useHistory, Route, Switch, Redirect } from 'react-router-dom';

import { Sidebar } from 'components';
import { Socket, UserData, User, Room } from 'logic';
import ROUTES from 'shared/Routes';
import { Chat, RoomPanel, CreationRoom, ScreenSelector, UserPanel } from './layouts';

import './Home.scss';

const RoomChat = () => {
  const [force, forceUpdate] = useState(0);

  const routeHistory = useHistory();

  const onCreateRoom = () => {
    routeHistory.push(ROUTES.CreateRoom.route);
  };

  const onRoomLeave = () => {
    UserData.setInRoom(new Room(-1, 'globals', false));
    console.log('UserData.inRoom = ', UserData.inRoom);
    forceUpdate(force + 1);
  };

  return (
    <div className="home__screen">
      <Sidebar>
        <RoomPanel
          onCreateRoom={onCreateRoom}
          onRoomLeave={onRoomLeave}
        />
      </Sidebar>
      <Chat />
    </div>
  );
};

const Home = () => {
  const [force, forceUpdate] = useState(0);

  const socketGetUsers = (data) => {
    UserData.user = User.fromJSON(data.data[0]);
    forceUpdate(force + 1);
    console.log("socketGetUsers userData = ", UserData.user);
  };

  useEffect(() => {
    Socket.addEventListener('getUsers', socketGetUsers);
    Socket.emit('getUsers', {
      Users: null,
      Select: ['name', 'avatar', 'avatarHash'],
    });
    return () => Socket.removeEventListener('getUsers', socketGetUsers);
  }, []);

  return (
    <div className="home">
      <Redirect exact path="/home" to={ROUTES.RoomChat.route} />
      <ScreenSelector />
      <Switch>
        <Route path={ROUTES.RoomChat.route} component={RoomChat} />
        <Route path={ROUTES.UserPanel.route} component={UserPanel} />
        <Route path={ROUTES.CreateRoom.route}>
          <CreationRoom
            onRoomCreate={(_) => forceUpdate(force + 1)}
          />
        </Route>
      </Switch>
    </div>
  );
};

export default Home;
