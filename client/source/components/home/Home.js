import React, { useState } from 'react';

import Chat from '../chat/Chat';

import './Home.scss';

const Home = () => {
    return (
        <div className="home">
            <Chat></Chat>
        </div>
    )
}

export default Home;