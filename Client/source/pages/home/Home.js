import React, { useState, useEffect } from 'react';

import { Sidebar } from 'components';
import { Chat, RoomPanel, CreationRoom, UserWidget } from './layouts';

import './Home.scss';

const Home = () => {
    const [stage, setStage] = useState('chat');

    const onCreateRoom = () => {
        setStage('creation_room');
    }

    let content = null;
    if (stage === 'creation_room') {
        content = <CreationRoom onComplete={() => setStage('chat')}></CreationRoom>
    }

    return (
        <div className="home">
            <Sidebar>
                <UserWidget />
                <RoomPanel onCreateRoom={onCreateRoom}></RoomPanel>
            </Sidebar>
            <Chat></Chat>
            {content}
        </div>
    )
}

export default Home;