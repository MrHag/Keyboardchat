import React, { useState } from 'react';

import Socket from '../../Socket';
import { TextareaAutosize, IconButton  } from '@material-ui/core/';

import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import * as FontAwesomeIcons from '@fortawesome/free-solid-svg-icons';

import './ChatInput.scss';

const ChatInput = () => {
    const [text, setText] = useState('');

    const onChatKeydown = (e) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            sendMessage();
            e.preventDefault();
        }
    }

    const sendMessage = () => {
        setText('');
        Socket.emit('chat', {
            message: text
        });
    }
    
    return (
        <div className="chat-input">
            <TextareaAutosize className="chat-input__area" variant="outlined" rowsMax={4} rowsMin={4} value={text} autoFocus={true} placeholder="Your message"
                    onChange={e => setText(e.target.value)} onKeyDown={e => onChatKeydown(e)}></TextareaAutosize>
            <div className="chat-input__controls">
                <IconButton className="chat-input__send" onClick={sendMessage}><FontAwesomeIcon icon={FontAwesomeIcons.faPaperPlane}></FontAwesomeIcon></IconButton>
            </div>
        </div>
    )
}

export default ChatInput;