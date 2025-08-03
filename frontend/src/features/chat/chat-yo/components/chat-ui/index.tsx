import React, { useState } from "react";
import { MessageBox, Input } from "react-chat-elements";
import { useChat } from "../../hooks";
import type { ChatMessage } from "../../hooks";
import { useStyles } from "./style";
import avatar from "./assets/avatar1.png";
import "react-chat-elements/dist/main.css";

interface ChatUiProps {
    messages: ChatMessage[] | undefined;
    loading?: boolean;
    avatarMode?: boolean;
    handleSendMessage?: () => void;
    handlePlay?: () => void;
}

export const ChatUi = ({
    messages,
    loading = false,
    avatarMode = false,
    handleSendMessage,
    handlePlay
}: ChatUiProps) => {
    const classes = useStyles();
    const [input, setInput] = useState("");
    //const { sendMessage, loading, messages } = useChat();
    const avatarUrl = avatar;

    if (!avatarMode) {
        return (
            <div className={classes.chatWrapper}>
                <div className={classes.messagesList}>
                    {messages?.map((msg, i) => (
                        <MessageBox
                            className={classes.messageBox}
                            styles={{
                                backgroundColor: msg.position === "right" ? "#11bbff" : "#FFFFFF",
                                color: "#000",
                            }}
                            key={i}
                            id={i.toString()}
                            position={msg.position}
                            type="text"
                            text={msg.text}
                            title={msg.position === "right" ? "Me" : "Assistant"}
                            titleColor={msg.position === "right" ? "black" : "gray"}
                            date={msg.date}
                            forwarded={false}
                            replyButton={true}
                            removeButton={true}
                            status="received"
                            notch={true}
                            focus={false}
                            retracted={false}
                            avatar={msg.position === "left" ? avatarUrl : undefined}
                        />
                    ))}
                    {loading && (
                        <MessageBox
                            id="assistant"
                            position="left"
                            type="text"
                            text="Thinking..."
                            title="Assistant"
                            titleColor="none"
                            date={new Date()}
                            forwarded={false}
                            replyButton={false}
                            removeButton={false}
                            status={"waiting"}
                            notch={true}
                            focus={false}
                            retracted={false}
                        />
                    )}
                </div>

                <Input
                    placeholder="Type a message..."
                    className={classes.input}
                    value={input}
                    onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                        setInput(e.target.value)
                    }
                    maxHeight={100}
                    onKeyDown={(e) => e.key === "Enter" && handleSendMessage?.()}
                    rightButtons={
                        <button className={classes.sendButton} onClick={handleSendMessage}>
                            {loading ? "..." : "↑"}
                        </button>
                    }
                />
            </div>
        );

    }

};
