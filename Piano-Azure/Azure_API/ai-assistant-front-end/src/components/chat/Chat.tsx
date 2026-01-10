import type { ChangeEvent, FormEvent } from "react";
import { useEffect, useRef, useState } from "react";
import "./Chat.css";

import { post } from "./../../services/api-client.service";

type ButtonState = "Enable" | "Disable";
type SenderType = "Client" | "Assistant";

interface Message {
    text: string;
    senderType: SenderType;
}

interface PostMessageRequestDTO {
    textMessage: string;
}
interface PostMessageResponseDTO {
    textMessage: string;
}

const Chat: React.FC = () => {
    const [message, setMessage] = useState<string>("");
    const [messages, setMessages] = useState<Message[]>([]);
    const messagesReference = useRef(messages);
    const [buttonState, setButtonState] = useState<ButtonState>("Enable");

    useEffect(() => {
        messagesReference.current = messages;
    }, [messages]);

    const handleInputChangeEvent = (event: ChangeEvent<HTMLInputElement>) => {
        setMessage(event.target.value);
    };

    const handleSubmitEvent = (event: FormEvent) => {
        event.preventDefault();
        setButtonState("Disable");

        setMessages(
            [...messagesReference.current, {
                text: message,
                senderType: "Client"
            }]
        );

        post<PostMessageRequestDTO, PostMessageResponseDTO>(
            "AIAssistant/message",
            {
                textMessage: message
            }
        ).then(
            (response: PostMessageResponseDTO) => {
                setMessages(
                    [...messagesReference.current, {
                        text: response.textMessage,
                        senderType: "Assistant"
                    }]
                );
                setMessage("");
                setButtonState("Enable");
            }
        );
    }

    return (
        <div className="main-container">
            <div className="messages-container">
                {
                    messages.map((m, index) => (
                        <div key={index} className={m.senderType.toLowerCase() + "-message message"}>
                            {m.text}
                        </div>
                    ))
                }
            </div>

            <form onSubmit={handleSubmitEvent} className="message-form">
                <input className="message-input" type="text" value={message} onChange={handleInputChangeEvent} />
                <button className="send-button" type="submit" disabled={buttonState === "Disable"}>
                    Send
                </button>
            </form>
        </div>
    );
}

export default Chat;