import React, { useState } from "react";
import { MessageBox, Input } from "react-chat-elements";
import type { ChatMessage } from "../../hooks";
import { useStyles } from "./style";
import avatar from "../../assets/avatar1.png";
import "react-chat-elements/dist/main.css";

interface ChatUiProps {
    messages: ChatMessage[] | undefined;
    loading: boolean;
    avatarMode?: boolean;
    value: string;
    onChange: (v: string) => void;
    handleSendMessage: () => void;
    handlePlay?: () => void;
}

export const ChatUi = ({
    messages,
    loading,
    avatarMode = false,
    value,
    onChange,
    handleSendMessage,
    handlePlay
}: ChatUiProps) => {
    const classes = useStyles();
    //const [input, setInput] = useState("");;
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
                            replyButton={false}
                            removeButton={false}
                            status="received"
                            focus={false}
                            retracted={false}
                            avatar={msg.position === "left" ? avatarUrl : undefined}
                            notch
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
                            focus={false}
                            retracted={false}
                            notch
                        />
                    )}
                </div>

                <Input
                    placeholder="Type a message..."
                    className={classes.input}
                    value={value}
                    maxHeight={100}
                    onChange={(e: React.ChangeEvent<HTMLInputElement>) => onChange(e.target.value)}
                    onKeyDown={(e) => e.key === "Enter" && handleSendMessage()}
                    rightButtons={
                        <button className={classes.sendButton} onClick={handleSendMessage}>
                            {loading ? "..." : "↑"}
                        </button>
                    }
                />
            </div>
        );

    }
    return (
        <>
            <div className={classes.messagesListAvatar}>
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
                        replyButton={false}
                        removeButton={false}
                        status="received"
                        notch={true}
                        focus={false}
                        retracted={false}
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
                        focus={false}
                        retracted={false}
                        notch

                    />
                )}
            </div>

            <div className={classes.inputContainer}>
                <Input
                    placeholder="Type a message..."
                    className={classes.input}
                    value={value}
                    maxHeight={100}
                    onChange={(e: React.ChangeEvent<HTMLInputElement>) => onChange(e.target.value)}
                    onKeyDown={(e) => e.key === "Enter" && handleSendMessage()}
                    rightButtons={
                        <div className={classes.rightButtons}>
                            <button
                                className={classes.sendButton}
                                onClick={handlePlay}
                            >
                                {loading ? "..." : "🗣"}
                            </button>
                            <button className={classes.sendButton} onClick={handleSendMessage}>
                                {loading ? "..." : "↑"}
                            </button>
                        </div>
                    }
                />

            </div>
        </>
    );
};
