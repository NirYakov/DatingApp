import { EventEmitter } from '@angular/core';
export declare class NgxGalleryActionComponent {
    icon: string;
    disabled: boolean;
    titleText: string;
    onClick: EventEmitter<Event>;
    handleClick(event: Event): void;
}
