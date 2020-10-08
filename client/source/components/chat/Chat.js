import React, { useEffect, useState } from 'react';

import Socket from '../../Socket';
import { ChatMessage, ChatInput } from '../index';

import './Chat.scss';

const fake_messages = [
    { name: "Vasya", message: "Hello world!", avatar: "images/unknown.png" },
    { name: "Server", message: "Vasya is connected", avatar: "images/server.jpg" },
    { name: "Vasya", message: "Графиня была женщина с восточным типом худого лица, лет сорока пяти, видимо, изнуренная детьми, которых у ней было двенадцать человек. Медлительность ее движений и говора, происходившая от слабости сил, придавала ей значительный вид, внушавший уважение. Княгиня Анна Михайловна Друбецкая, как домашний человек, сидела тут же, помогая в деле принимания и занимания разговором гостей. Молодежь была в задних комнатах, не находя нужным участвовать в приеме визитов. Граф встречал и провожал гостей, приглашая всех к обеду.", avatar: "images/unknown.png" },
    { name: "Server", message: "Между тем все это молодое поколение: Борис — офицер, сын княгини Анны Михайловны, Николай — студент, старший сын графа, Соня — пятнадцатилетняя племянница графа, и маленький Петруша — меньшой сын, — все разместились в гостиной и, видимо, старались удержать в границах приличия оживление и веселость, которыми еще дышала каждая их черта. Видно было, что там, в задних комнатах, откуда они все так стремительно прибежали, у них были разговоры веселее, чем здесь о городских сплетнях, погоде и comtesse Apraksine 3. Изредка они взглядывали друг на друга и едва удерживались от смеха.", avatar: "images/server.jpg" },
    { name: "Vasya", message: "Hello world!", avatar: "images/unknown.png" },
    { name: "Server", message: "Vasya is connected", avatar: "images/server.jpg" },
    { name: "Vasya", message: "Графиня была женщина с восточным типом худого лица, лет сорока пяти, видимо, изнуренная детьми, которых у ней было двенадцать человек. Медлительность ее движений и говора, происходившая от слабости сил, придавала ей значительный вид, внушавший уважение. Княгиня Анна Михайловна Друбецкая, как домашний человек, сидела тут же, помогая в деле принимания и занимания разговором гостей. Молодежь была в задних комнатах, не находя нужным участвовать в приеме визитов. Граф встречал и провожал гостей, приглашая всех к обеду.", avatar: "images/unknown.png" },
    { name: "Server", message: "ФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФ фолыва жыдвлоа жфывд алофыжв дклаоф жвадлофы жвдало фывжд алофывжа длфоывжадфлоываждфволажфдывалофждываол фжвыдало фвжад лоы", avatar: "images/server.jpg" },
    { name: "Vasya", message: "Hello world!", avatar: "images/unknown.png" },
    { name: "Server", message: "Vasya is connected", avatar: "images/server.jpg" },
    { name: "Vasya", message: "Князь Василий исполнил обещание, данное на вечере у Анны Павловны княгине Друбецкой, просившей его о своем единственном сыне Борисе. О нем было доложено государю, и, не в пример другим, он был переведен в гвардии Семеновский полк прапорщиком. Но адъютантом или состоящим при Кутузове Борис так и не был назначен, несмотря на все хлопоты и происки Анны Михайловны. Вскоре после вечера Анны Павловны Анна Михайловна вернулась в Москву, прямо к своим богатым родственникам Ростовым, у которых она стояла в Москве и у которых с детства воспитывался и годами живал ее обожаемый Боренька, только что произведенный в армейские и тотчас переведенный в гвардейские прапорщики. Гвардия уже вышла из Петербурга 10-го августа, и сын, оставшийся для обмундирования в Москве, должен был догнать ее по дороге в Радзивилов.", avatar: "images/unknown.png" },
    { name: "Server", message: "ФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФФ фолыва жыдвлоа жфывд алофыжв дклаоф жвадлофы жвдало фывжд алофывжа длфоывжадфлоываждфволажфдывалофждываол фжвыдало фвжад лоы", avatar: "images/server.jpg" },
]

const Chat = () => {
    //Stores messages in JSON format
    const [messages, setMessages] = useState((fake_messages !== undefined) ? fake_messages : []);
    const historyRef = React.useRef();

    const onNewMessage = data => {
        setMessages([...messages, data]);
        historyRef.current.scrollTop = historyRef.current.scrollHeight;
    }

    useEffect(() => {
        Socket.on('chat', onNewMessage);
        historyRef.current.scrollTop = historyRef.current.scrollHeight;
        return () => Socket.off('chat', onNewMessage);
    });

    return (
        <div className="chat">
            <div className="chat__history-wrapper">
                <div ref={historyRef} className="chat__history">
                    { messages.map((msg, index) => <ChatMessage key={index} msg={msg}></ChatMessage>) }
                </div>
            </div>
            <ChatInput></ChatInput>
        </div>
    )
}

export default Chat;