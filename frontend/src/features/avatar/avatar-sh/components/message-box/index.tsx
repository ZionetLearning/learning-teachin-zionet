import { MessageBox as Message, Input } from "react-chat-elements";
import type { ChatMessage } from "../../../../chat/chat-yo/hooks/useChat";

interface MessageBoxProps {
  message: ChatMessage | undefined;
  key?: number;
  loading?: boolean;
  className?: string;
}
export const MessageBox = ({
  message,
  key = 0,
  loading = false,
  className,
}: MessageBoxProps) => {
  {
    if (loading === false) {
      return (<Message
        className={className}
        styles={{
          backgroundColor:
            message?.position === "right" ? "#11bbff" : "#FFFFFF",
          color: "#000",
        }}
        key={key}
        id={key.toString()}
        position={message?.position ?? ""}
        type="text"
        text={message?.text ?? ""}
        title={message?.position === "right" ? "Me" : "Assistant"}
        titleColor={message?.position === "right" ? "black" : "gray"}
        date={message?.date ?? new Date()}
        forwarded={false}
        replyButton={true}
        removeButton={true}
        status="received"
        notch={true}
        focus={false}
        retracted={false}
      ></Message>);
    }
    return (<Message
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
      status="waiting"
      notch={true}
      focus={false}
      retracted={false}
    ></Message>);
  }
}
