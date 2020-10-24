import React, { useState } from 'react';
import { useHistory, Route, Switch, Redirect } from 'react-router-dom';

import { Sidebar } from 'components';
import { Chat, RoomPanel, CreationRoom, ScreenSelector, UserPanel } from './layouts';

import './Home.scss';
import ROUTES from '../../shared/Routes';

const RoomChat = () => {
    const routeHistory = useHistory();

    const onCreateRoom = () => {
        console.log("hello world!");
        routeHistory.push(ROUTES.CreateRoom.route);
    }

    return (
        <div className="home__screen">
            <Sidebar>
                <RoomPanel onCreateRoom={onCreateRoom}></RoomPanel>
            </Sidebar>
            <Chat></Chat>
        </div>
    )
}

const Home = () => {
    const [stage, setStage] = useState('chat');

    let content = null;
    if (stage === 'creation_room') {
        content = <CreationRoom onComplete={() => setStage('chat')}></CreationRoom>
    }

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