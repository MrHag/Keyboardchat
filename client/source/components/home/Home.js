import React, { useState, useEffect } from 'react';
import { Chat, RoomPanel, CreationRoom } from '../index';

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
            <RoomPanel onCreateRoom={onCreateRoom}></RoomPanel>
            <Chat></Chat>
            {content}
        </div>
    )
}

export default Home;