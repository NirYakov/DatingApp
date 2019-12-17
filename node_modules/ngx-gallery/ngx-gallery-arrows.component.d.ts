import { EventEmitter } from '@angular/core';
export declare class NgxGalleryArrowsComponent {
    prevDisabled: boolean;
    nextDisabled: boolean;
    arrowPrevIcon: string;
    arrowNextIcon: string;
    onPrevClick: EventEmitter<{}>;
    onNextClick: EventEmitter<{}>;
    handlePrevClick(): void;
    handleNextClick(): void;
}
