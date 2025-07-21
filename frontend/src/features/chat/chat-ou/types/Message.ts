export interface MessageSender {
  id: string;
  name: string;
  type: "user" | "ai";
  avatar?: string;
}

export interface MessageContext {
  pageUrl?: string;
  pageTitle?: string;
  selectedText?: string;
  metadata?: Record<string, any>;
}

export interface BaseMessage {
  id: string;
  timestamp: Date;
  sender: MessageSender;
  context?: MessageContext;
}

export interface TextMessage extends BaseMessage {
  type: "text";
  content: string;
}

export interface ImageMessage extends BaseMessage {
  type: "image";
  content: {
    url: string;
    alt: string;
    caption?: string;
  };
}

export interface GenerativeUIMessage extends BaseMessage {
  type: "generative-ui";
  content: {
    componentType: string;
    props: Record<string, any>;
    fallbackText: string;
  };
}

export type Message = TextMessage | ImageMessage | GenerativeUIMessage;
