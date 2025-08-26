import type {
  MockAIService,
  MockConversation,
  Message,
  MessageContext,
  MessageSender,
  TextMessage,
  ImageMessage,
  GenerativeUIMessage,
} from "../types";
import {
  AI_SENDER,
  USER_SENDER,
  TEXT_RESPONSE_TEMPLATES,
  TEXT_FOLLOWUP_MESSAGES,
  IMAGE_CAPTIONS,
  MOCK_QUIZZES,
  MOCK_LINKS,
  INITIAL_CONVERSATION_CONFIG,
  MESSAGE_PATTERNS,
  TIMING_CONFIG,
} from "./MockMessages";

export class MockChatService implements MockAIService {
  private readonly aiSender: MessageSender = AI_SENDER;

  private readonly userSender: MessageSender = USER_SENDER;

  private isTyping = false;

  getInitialConversation(): MockConversation {
    return {
      id: INITIAL_CONVERSATION_CONFIG.id,
      title: INITIAL_CONVERSATION_CONFIG.title,
      messages: [
        {
          id: INITIAL_CONVERSATION_CONFIG.welcomeMessage.id,
          type: INITIAL_CONVERSATION_CONFIG.welcomeMessage.type,
          content: INITIAL_CONVERSATION_CONFIG.welcomeMessage.content,
          timestamp: new Date(
            Date.now() -
              INITIAL_CONVERSATION_CONFIG.welcomeMessage.timestampOffset,
          ),
          sender: this.aiSender,
        },
      ],
      participants: [this.userSender, this.aiSender],
    };
  }

  async simulateNetworkDelay(): Promise<void> {
    const delay =
      Math.random() *
        (TIMING_CONFIG.networkDelay.max - TIMING_CONFIG.networkDelay.min) +
      TIMING_CONFIG.networkDelay.min;
    return new Promise((resolve) => setTimeout(resolve, delay));
  }
  async generateResponse(
    userMessage: string,
    context?: MessageContext,
  ): Promise<Message> {
    await this.simulateTyping();

    const messageId = `ai-${Date.now()}`;
    const timestamp = new Date();
    const lowerMessage = userMessage.toLowerCase();

    if (
      MESSAGE_PATTERNS.image.some((keyword) => lowerMessage.includes(keyword))
    ) {
      return this.createImageResponse(
        messageId,
        timestamp,
        userMessage,
        context,
      );
    }

    if (
      MESSAGE_PATTERNS.quiz.some((keyword) => lowerMessage.includes(keyword))
    ) {
      return this.createQuizResponse(
        messageId,
        timestamp,
        userMessage,
        context,
      );
    }

    if (
      MESSAGE_PATTERNS.link.some((keyword) => lowerMessage.includes(keyword))
    ) {
      return this.createLinkResponse(messageId, timestamp, context);
    }

    return this.createTextResponse(messageId, timestamp, userMessage, context);
  }

  async simulateTyping(): Promise<void> {
    if (this.isTyping) return;

    this.isTyping = true;
    const delay =
      Math.random() *
        (TIMING_CONFIG.typingDelay.max - TIMING_CONFIG.typingDelay.min) +
      TIMING_CONFIG.typingDelay.min;
    await new Promise((resolve) => setTimeout(resolve, delay));
    this.isTyping = false;
  }

  private createTextResponse(
    messageId: string,
    timestamp: Date,
    userMessage: string,
    context?: MessageContext,
  ): TextMessage {
    let response = TEXT_RESPONSE_TEMPLATES[
      Math.floor(Math.random() * TEXT_RESPONSE_TEMPLATES.length)
    ].replace("{userMessage}", userMessage);

    response += `\n\n${TEXT_FOLLOWUP_MESSAGES[Math.floor(Math.random() * TEXT_FOLLOWUP_MESSAGES.length)]}`;

    if (context?.selectedText) {
      response += `\n\nI notice you selected: "${context.selectedText}".`;
    }

    return {
      id: messageId,
      type: "text",
      content: response,
      timestamp,
      sender: this.aiSender,
      context,
    };
  }

  private createImageResponse(
    messageId: string,
    timestamp: Date,
    _userMessage: string,
    context?: MessageContext,
  ): ImageMessage {
    const imageId = Math.floor(Math.random() * 100) + 1;

    return {
      id: messageId,
      type: "image",
      content: {
        url: `https://picsum.photos/400/300?random=${imageId}`,
        alt: "AI generated response image",
        caption:
          IMAGE_CAPTIONS[Math.floor(Math.random() * IMAGE_CAPTIONS.length)],
      },
      timestamp,
      sender: this.aiSender,
      context,
    };
  }

  private createQuizResponse(
    messageId: string,
    timestamp: Date,
    _userMessage: string,
    context?: MessageContext,
  ): GenerativeUIMessage {
    const randomQuiz =
      MOCK_QUIZZES[Math.floor(Math.random() * MOCK_QUIZZES.length)];

    return {
      id: messageId,
      type: "generative-ui",
      content: {
        componentType: "QuizMessage",
        props: randomQuiz,
        fallbackText: `Quiz: ${randomQuiz.question}`,
      },
      timestamp,
      sender: this.aiSender,
      context,
    };
  }

  private createLinkResponse(
    messageId: string,
    timestamp: Date,
    context?: MessageContext,
  ): GenerativeUIMessage {
    const randomLink =
      MOCK_LINKS[Math.floor(Math.random() * MOCK_LINKS.length)];

    return {
      id: messageId,
      type: "generative-ui",
      content: {
        componentType: "LinkMessage",
        props: randomLink,
        fallbackText: `Check out: ${randomLink.title} - ${randomLink.description}`,
      },
      timestamp,
      sender: this.aiSender,
      context,
    };
  }
}
