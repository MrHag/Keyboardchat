import React, { useState } from 'react';
import { useHistory, Route, Switch, Redirect } from 'react-router-dom';

import { Sidebar } from 'components';
import { Chat, RoomPanel, CreationRoom, ScreenSelector, UserPanel } from './layouts';

import './Home.scss';
import ROUTES from '../../shared/Routes';

const RoomChat = () => {
    const [force, forceUpdate] = useState(0);

    const routeHistory = useHistory();

    const onCreateRoom = () => {
        routeHistory.push(ROUTES.CreateRoom.route);
    }

    return (
        <div className="home__screen">
            <Sidebar>
                <RoomPanel
                    onCreateRoom={onCreateRoom}
                    onRoomLeave={_ => forceUpdate(force + 1)}
                />
            </Sidebar>
            <Chat></Chat>
        </div>
    )
}

const Home = () => {
    return (
        <div className="home">
            <Redirect exact path="/home" to={ROUTES.RoomChat.route}></Redirect>
            <ScreenSelector />
            <Switch>
                <Route path={ROUTES.RoomChat.route} component={RoomChat} />
                <Route path={ROUTES.UserPanel.route} component={UserPanel} />
                <Route path={ROUTES.CreateRoom.route} component={CreationRoom} />
            </Switch>
        </div>
    );
};

export default Home;