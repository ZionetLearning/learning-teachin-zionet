import type { MessageContext } from "../types";

export interface ContextService {
  getCurrentPageContext(): MessageContext;
  attachContext(message: string): MessageContext;
  extractSelectedText(): string | undefined;
  getPageMetadata(): Record<string, any>;
}

export class ContextServiceImpl implements ContextService {
  getCurrentPageContext(): MessageContext {
    const context: MessageContext = {
      pageUrl: window.location.href,
      pageTitle: document.title,
      selectedText: this.extractSelectedText(),
      metadata: this.getPageMetadata(),
    };

    return context;
  }

  attachContext(message: string): MessageContext {
    const baseContext = this.getCurrentPageContext();

    // Add message-specific context
    return {
      ...baseContext,
      metadata: {
        ...baseContext.metadata,
        messageLength: message.length,
        timestamp: new Date().toISOString(),
        userAgent: navigator.userAgent,
      },
    };
  }

  extractSelectedText(): string | undefined {
    const selection = window.getSelection();
    if (selection && selection.toString().trim()) {
      return selection.toString().trim();
    }
    return undefined;
  }

  getPageMetadata(): Record<string, any> {
    const metadata: Record<string, any> = {
      url: window.location.href,
      pathname: window.location.pathname,
      search: window.location.search,
      hash: window.location.hash,
      referrer: document.referrer,
      timestamp: new Date().toISOString(),
    };

    // Extract meta tags
    const metaTags = document.querySelectorAll("meta");
    const metaData: Record<string, string> = {};

    metaTags.forEach((tag) => {
      const name = tag.getAttribute("name") || tag.getAttribute("property");
      const content = tag.getAttribute("content");

      if (name && content) {
        metaData[name] = content;
      }
    });

    if (Object.keys(metaData).length > 0) {
      metadata.metaTags = metaData;
    }

    // Get viewport dimensions
    metadata.viewport = {
      width: window.innerWidth,
      height: window.innerHeight,
    };

    return metadata;
  }

  // Utility method to format context for display
  formatContextForDisplay(context: MessageContext): string {
    const parts: string[] = [];

    if (context.pageTitle) {
      parts.push(`📄 ${context.pageTitle}`);
    }

    if (context.selectedText) {
      parts.push(`📝 "${context.selectedText}"`);
    }

    if (context.pageUrl && context.pageUrl !== window.location.href) {
      parts.push(`🔗 ${context.pageUrl}`);
    }

    return parts.join("\n");
  }

  // Check if context has meaningful data
  hasSignificantContext(context: MessageContext): boolean {
    return !!(
      context.selectedText ||
      (context.pageTitle && context.pageTitle !== "Untitled") ||
      (context.metadata && Object.keys(context.metadata).length > 3)
    );
  }
}
